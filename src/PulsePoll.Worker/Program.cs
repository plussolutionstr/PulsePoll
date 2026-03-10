using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using PulsePoll.Application;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Infrastructure;
using PulsePoll.Infrastructure.Storage;
using PulsePoll.Worker.BackgroundServices;
using PulsePoll.Worker.Jobs;
using PulsePoll.Worker.Services;
using PulsePoll.Worker.Consumers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IMediaUrlService, PresignedMediaUrlService>();

var postgresConnection = builder.Configuration.GetConnectionString("Postgres")
                         ?? throw new InvalidOperationException("Postgres connection string is missing.");

builder.Services.AddHangfire(cfg => cfg
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(postgresConnection)));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<CommunicationAutomationRecurringJob>();
builder.Services.AddScoped<ICommunicationAutomationJobScheduler, CommunicationAutomationJobScheduler>();
builder.Services.AddHostedService<CommunicationAutomationSchedulerBootstrapService>();
builder.Services.AddScoped<ReferralRewardReconciliationRecurringJob>();
builder.Services.AddScoped<AffiliateRewardReconciliationRecurringJob>();
builder.Services.AddScoped<IReferralRewardJobScheduler, ReferralRewardJobScheduler>();
builder.Services.AddHostedService<ReferralRewardSchedulerBootstrapService>();

builder.Services.AddScoped<SurveyDistributionRecurringJob>();
builder.Services.AddScoped<SurveyReminderRecurringJob>();
builder.Services.AddScoped<ISurveyDistributionJobScheduler, SurveyDistributionJobScheduler>();
builder.Services.AddHostedService<SurveyDistributionSchedulerBootstrapService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SubjectRegisteredConsumer>();
    x.AddConsumer<SurveyCompletedConsumer>();
    x.AddConsumer<NotificationSendConsumer>();
    x.AddConsumer<NotificationSendFaultConsumer>();
    x.AddConsumer<WithdrawalRequestedConsumer>();
    x.AddConsumer<WalletCreditConsumer>();
    x.AddConsumer<SubjectAssignmentConsumer>();
    x.AddConsumer<SmsSendConsumer>();
    x.AddConsumer<SmsSendFaultConsumer>();
    x.AddConsumer<CommunicationAutomationScheduleChangedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitCfg = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbitCfg["Host"], rabbitCfg["VirtualHost"] ?? "/", h =>
        {
            h.Username(rabbitCfg["Username"]!);
            h.Password(rabbitCfg["Password"]!);
        });

        cfg.ReceiveEndpoint(Queues.SubjectRegistered, e => e.ConfigureConsumer<SubjectRegisteredConsumer>(ctx));
        cfg.ReceiveEndpoint(Queues.SurveyCompleted, e => e.ConfigureConsumer<SurveyCompletedConsumer>(ctx));
        cfg.ReceiveEndpoint(Queues.NotificationSend, e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<NotificationSendConsumer>(ctx);
        });
        cfg.ReceiveEndpoint(Queues.NotificationSendFault, e => e.ConfigureConsumer<NotificationSendFaultConsumer>(ctx));
        cfg.ReceiveEndpoint(Queues.WithdrawalRequested, e => e.ConfigureConsumer<WithdrawalRequestedConsumer>(ctx));
        cfg.ReceiveEndpoint(Queues.WalletCredit, e => e.ConfigureConsumer<WalletCreditConsumer>(ctx));
        cfg.ReceiveEndpoint(Queues.SubjectAssignmentRequested, e => e.ConfigureConsumer<SubjectAssignmentConsumer>(ctx));
        cfg.ReceiveEndpoint(Queues.SmsSend, e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<SmsSendConsumer>(ctx);
        });
        cfg.ReceiveEndpoint(Queues.SmsSendFault, e => e.ConfigureConsumer<SmsSendFaultConsumer>(ctx));
        cfg.ReceiveEndpoint(Queues.CommunicationAutomationScheduleChanged, e =>
            e.ConfigureConsumer<CommunicationAutomationScheduleChangedConsumer>(ctx));
    });
});

var host = builder.Build();
host.Run();
