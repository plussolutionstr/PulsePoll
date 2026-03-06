using System.Security.Claims;
using System.Net;
using System.Net.Sockets;
using System.Threading.RateLimiting;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using PulsePoll.Admin.Components;
using PulsePoll.Admin.Services;
using PulsePoll.Application;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;
using PulsePoll.Infrastructure;
using PulsePoll.Infrastructure.Persistence;
using PulsePoll.Infrastructure.Persistence.Seeding;
using PulsePoll.Infrastructure.Storage;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => { options.SizeMode = DevExpress.Blazor.SizeMode.Medium; });
builder.Services.AddMvc();

// Validators (FluentValidation)
builder.Services.AddApplication();

// Infrastructure (DB, Redis, MinIO, Repositories + JWT registered — overridden below)
builder.Services.AddInfrastructure(builder.Configuration);

// MassTransit – publish only (Admin, WalletService/NotificationService RabbitMQ'ya mesaj gönderir)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((_, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], builder.Configuration["RabbitMQ:VirtualHost"] ?? "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
    });
});

// Media URL: Admin proxy endpoint üzerinden serve eder (presigned URL Docker ağında kalır)
builder.Services.AddScoped<IMediaUrlService, ProxyMediaUrlService>();

// Application services
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IAppToastService, AppToastService>();

// Cookie Authentication (overrides JWT default set by AddInfrastructure)
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath            = "/login";
        options.LogoutPath           = "/logout";
        options.AccessDeniedPath     = "/access-denied";
        options.ExpireTimeSpan       = TimeSpan.FromHours(8);
        options.SlidingExpiration    = true;
        options.Cookie.Name          = "PulsePoll.Admin.Auth";
        options.Cookie.HttpOnly      = true;
        options.Cookie.SecurePolicy  = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite      = SameSiteMode.Lax;
    });

// Permission authorization (Blazor-specific, reads from cookie claims)
builder.Services.AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>();

// Rate limiting (admin login)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("admin-login", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            IpResolver.ResolveClientIp(ctx),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(5)
            }));

    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.ContentType = "application/json";

        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { success = false, errorMessage = "Çok fazla giriş denemesi. Lütfen bir süre bekleyin." }, ct);
    };
});

// Required for Blazor Server auth
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Ensure DB schema is up-to-date for Admin modules.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Seed permissions + default SuperAdmin role + initial admin user
await PermissionSeeder.SeedAsync(app.Services);
try
{
    using var scope = app.Services.CreateScope();
    var permissionCacheService = scope.ServiceProvider.GetRequiredService<IAdminPermissionCacheService>();
    await permissionCacheService.RefreshAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Redis erişilemediği için admin permission cache warm-up atlandı.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();
app.MapStaticAssets();

// Login endpoint
app.MapPost("/api/auth/login", async (
    HttpContext httpContext,
    IAdminAuthService authService,
    LoginRequest request) =>
{
    var user = await authService.ValidateCredentialsAsync(request.Email, request.Password);
    if (user is null)
        return Results.Ok(new { success = false, errorMessage = "E-posta veya şifre hatalı." });

    var permCodes = await authService.GetPermissionCodesAsync(user.Id);

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new(ClaimTypes.Email, user.Email),
    };
    claims.AddRange(permCodes.Select(code => new Claim("perm", code)));

    var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    var authProps = new AuthenticationProperties
    {
        IsPersistent = request.RememberMe,
        ExpiresUtc   = request.RememberMe
            ? TurkeyTime.OffsetNow.AddDays(30)
            : TurkeyTime.OffsetNow.AddHours(8),
        AllowRefresh = true
    };

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

    return Results.Ok(new
    {
        success = true,
        redirectUrl = ReturnUrlSafety.Normalize(request.ReturnUrl)
    });
}).AllowAnonymous().DisableAntiforgery().RequireRateLimiting("admin-login");

// Logout endpoint
app.MapGet("/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).AllowAnonymous();

// Media proxy — serves MinIO objects through the admin app
app.MapGet("/api/media/{bucket}/{objectKey}", async (
    string bucket,
    string objectKey,
    IStorageService storageService) =>
{
    try
    {
        var (stream, contentType) = await storageService.GetObjectStreamAsync(bucket, objectKey);
        return Results.File(stream, contentType, enableRangeProcessing: true);
    }
    catch
    {
        return Results.NotFound();
    }
}).AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public record LoginRequest(string Email, string Password, bool RememberMe = false, string? ReturnUrl = null);

static class IpResolver
{
    public static string ResolveClientIp(HttpContext ctx)
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

static class ReturnUrlSafety
{
    public static string Normalize(string? returnUrl)
        => IsSafeLocalPath(returnUrl) ? returnUrl! : "/";

    private static bool IsSafeLocalPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!path.StartsWith('/'))
            return false;

        if (path.StartsWith("//", StringComparison.Ordinal))
            return false;

        if (path.Contains('\\'))
            return false;

        return !Uri.TryCreate(path, UriKind.Absolute, out _);
    }
}
