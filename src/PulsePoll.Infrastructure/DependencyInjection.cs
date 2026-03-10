using System.Text;
using EFCore.NamingConventions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Minio;
using PulsePoll.Application.Interfaces;
using PulsePoll.Infrastructure.Auth;
using PulsePoll.Infrastructure.Caching;
using PulsePoll.Infrastructure.Messaging;
using PulsePoll.Infrastructure.Notifications;
using PulsePoll.Infrastructure.Persistence;
using PulsePoll.Infrastructure.Persistence.Repositories;
using PulsePoll.Infrastructure.Services;
using PulsePoll.Infrastructure.Storage;
using StackExchange.Redis;

namespace PulsePoll.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // PostgreSQL
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Postgres"),
                npgsqlOpt => npgsqlOpt.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(1),
                    errorCodesToAdd: null))
               .UseSnakeCaseNamingConvention());

        // Blazor Server gridleri için (GridDevExtremeDataSource uzun ömürlü IQueryable tutar)
        services.AddDbContextFactory<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Postgres"),
                npgsqlOpt => npgsqlOpt.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(1),
                    errorCodesToAdd: null))
               .UseSnakeCaseNamingConvention(), ServiceLifetime.Scoped);

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(config.GetConnectionString("Redis")!);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = Math.Max(options.ConnectRetry, 3);
            if (options.ConnectTimeout <= 0)
                options.ConnectTimeout = 5000;

            return ConnectionMultiplexer.Connect(options);
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // MinIO
        var useSSL = config.GetValue<bool>("MinIO:UseSSL");
        services.AddMinio(client => client
            .WithEndpoint(config["MinIO:Endpoint"])
            .WithCredentials(config["MinIO:AccessKey"], config["MinIO:SecretKey"])
            .WithSSL(useSSL)
            .Build());
        services.AddScoped<IStorageService, MinioStorageService>();

        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ITaxOfficeRepository, TaxOfficeRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubjectAppActivityRepository, SubjectAppActivityRepository>();
        services.AddScoped<ISubjectScoreSnapshotRepository, SubjectScoreSnapshotRepository>();
        services.AddScoped<ISubjectScoreConfigRepository, SubjectScoreConfigRepository>();
        services.AddScoped<IRewardUnitConfigRepository, RewardUnitConfigRepository>();
        services.AddScoped<IReferralRewardConfigRepository, ReferralRewardConfigRepository>();
        services.AddScoped<IRegistrationConfigRepository, RegistrationConfigRepository>();
        services.AddScoped<IAppContentConfigRepository, AppContentConfigRepository>();
        services.AddScoped<ICommunicationAutomationConfigRepository, CommunicationAutomationConfigRepository>();
        services.AddScoped<IMessageAutomationRepository, MessageAutomationRepository>();
        services.AddScoped<ISpecialDayRepository, SpecialDayRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<ISesCalculator, SesCalculator>();
        services.AddScoped<IWithdrawalRequestRepository, WithdrawalRequestRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IAccessControlRepository, AccessControlRepository>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ISubjectAssignmentJobRepository, SubjectAssignmentJobRepository>();
        services.AddScoped<ISmsLogRepository, SmsLogRepository>();
        services.AddScoped<IPaymentBatchRepository, PaymentBatchRepository>();
        services.AddScoped<IPaymentSettingRepository, PaymentSettingRepository>();
        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<IAdminGridDataService, AdminGridDataService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDistributionLogRepository, DistributionLogRepository>();
        services.AddScoped<INotificationDistributionConfigRepository, NotificationDistributionConfigRepository>();
        services.AddScoped<IExternalAffiliateRepository, ExternalAffiliateRepository>();

        // Auth
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // JWT Bearer
        var jwtSettings = config.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds)
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    if (ctx.Exception is SecurityTokenExpiredException)
                        ctx.Response.Headers["Token-Expired"] = "true";
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        // IMessagePublisher – MassTransit ISendEndpointProvider'a bağlı.
        // AddMassTransit host tarafında (API veya Worker) çağrılmalıdır.
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        // Notifications
        services.AddSingleton<FcmPushService>();
        services.AddSingleton<SmtpMailService>();

        var twilioSection = config.GetSection(TwilioSettings.SectionName);
        if (twilioSection.Exists() && !string.IsNullOrEmpty(twilioSection["AccountSid"]))
        {
            services.Configure<TwilioSettings>(twilioSection);
            services.AddScoped<ISmsService, TwilioSmsService>();
        }
        else
        {
            services.AddScoped<ISmsService, MockSmsService>();
        }

        return services;
    }
}
