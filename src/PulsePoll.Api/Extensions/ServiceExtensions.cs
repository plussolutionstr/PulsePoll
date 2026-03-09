using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.RateLimiting;
using MassTransit;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Api.Models;
using PulsePoll.Api.Services;
using PulsePoll.Application;
using PulsePoll.Application.Interfaces;
using PulsePoll.Infrastructure;
using PulsePoll.Infrastructure.Messaging;

namespace PulsePoll.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddApplication();
        services.AddInfrastructure(config);
        services.AddScoped<IMediaUrlService, ProxyMediaUrlService>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole("Admin")
                    || ctx.User.HasClaim(c => c.Type == "perm"));
            });
        });

        var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(opt => opt.AddPolicy("Default", p =>
        {
            if (allowedOrigins.Length > 0)
            {
                p.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                return;
            }

            p.WithOrigins("https://localhost:5001", "http://localhost:5001", "http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));

        services.AddApiRateLimiter();

        // MassTransit – publish only (consumer'lar Worker'da)
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((_, cfg) =>
            {
                cfg.Host(config["RabbitMQ:Host"], config["RabbitMQ:VirtualHost"] ?? "/", h =>
                {
                    h.Username(config["RabbitMQ:Username"]!);
                    h.Password(config["RabbitMQ:Password"]!);
                });
            });
        });
        services.AddHostedService<OutboxDispatcherBackgroundService>();

        return services;
    }

    private static void AddApiRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // --- Named fixed-window policies ---
            AddFixedPolicy(options, "otp",           partitionByIp: true,  limit: 3,  window: TimeSpan.FromMinutes(1));
            AddFixedPolicy(options, "auth",          partitionByIp: true,  limit: 5,  window: TimeSpan.FromMinutes(1));
            AddFixedPolicy(options, "auth-register", partitionByIp: true,  limit: 3,  window: TimeSpan.FromMinutes(5));
            AddFixedPolicy(options, "token-refresh", partitionByIp: true,  limit: 10, window: TimeSpan.FromMinutes(1));
            AddFixedPolicy(options, "withdrawal",    partitionByIp: false, limit: 3,  window: TimeSpan.FromHours(1));
            AddFixedPolicy(options, "bank-add",      partitionByIp: false, limit: 5,  window: TimeSpan.FromHours(1));
            AddFixedPolicy(options, "webhook",       partitionByIp: true,  limit: 30, window: TimeSpan.FromMinutes(1));
            AddFixedPolicy(options, "project-start", partitionByIp: false, limit: 10, window: TimeSpan.FromMinutes(1));
            AddFixedPolicy(options, "survey-helper", partitionByIp: false, limit: 60, window: TimeSpan.FromMinutes(1));
            AddFixedPolicy(options, "telemetry",     partitionByIp: false, limit: 30, window: TimeSpan.FromMinutes(1));
            AddFixedPolicy(options, "survey-result", partitionByIp: true,  limit: 60, window: TimeSpan.FromMinutes(1));

            // --- Global limiter ---
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                var userId = ctx.User?.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"user_{userId}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
                            Window = TimeSpan.FromMinutes(1)
                        });
                }

                var ip = ResolveClientIp(ctx);
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"ip_{ip}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // --- OnRejected: standart envelope 429 ---
            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType = "application/json";

                var requestId = ctx.HttpContext.TraceIdentifier;
                var error = ApiResponseFactory.Error(
                    ErrorCodes.RateLimitExceeded,
                    "Çok fazla istek gönderdiniz. Lütfen bir süre bekleyin.",
                    requestId);

                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                await ctx.HttpContext.Response.WriteAsJsonAsync(error, ct);
            };
        });
    }

    private static void AddFixedPolicy(RateLimiterOptions options, string policyName, bool partitionByIp, int limit, TimeSpan window)
    {
        options.AddPolicy(policyName, ctx =>
        {
            var partitionKey = partitionByIp
                ? $"ip:{ResolveClientIp(ctx)}"
                : $"user:{ctx.User?.FindFirst("sub")?.Value ?? ResolveClientIp(ctx)}";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limit,
                    Window = window,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        });
    }

    private static string ResolveClientIp(HttpContext ctx)
    {
        var remoteIp = ctx.Connection.RemoteIpAddress;

        // Trust X-Forwarded-For only when request comes from a private/loopback proxy.
        if (IsPrivateOrLoopback(remoteIp))
        {
            var xff = ctx.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(xff))
            {
                var first = xff
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(first) && IPAddress.TryParse(first, out var forwardedIp))
                    return forwardedIp.ToString();
            }
        }

        return remoteIp?.ToString() ?? "unknown";
    }

    private static bool IsPrivateOrLoopback(IPAddress? ip)
    {
        if (ip is null)
            return false;

        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            return bytes[0] == 10
                   || (bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
                   || (bytes[0] == 192 && bytes[1] == 168)
                   || bytes[0] == 127;
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal;

        return false;
    }
}
