using Microsoft.AspNetCore.HttpOverrides;
using PulsePoll.Api.Extensions;
using PulsePoll.Api.Logging;
using PulsePoll.Api.Middleware;
using PulsePoll.Infrastructure;
using PulsePoll.Infrastructure.Persistence;
using PulsePoll.Infrastructure.Persistence.Seeding;
using PulsePoll.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.With(new UserContextEnricher(services.GetRequiredService<IHttpContextAccessor>()))
    .WriteTo.Console()
    .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

builder.Services.AddControllers();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}
await PermissionSeeder.SeedAsync(app.Services);

// Seed sonrası access-control cache'lerini temizle (Redis'te eski permission listesi kalmasın)
try
{
    using var cacheScope = app.Services.CreateScope();
    var cacheService = cacheScope.ServiceProvider.GetRequiredService<ICacheService>();
    await cacheService.RemoveAsync("admin:access-control:permissions:v2");
    await cacheService.RemoveAsync("admin:access-control:roles:v2");
    await cacheService.RemoveAsync("admin:access-control:role-lookups:v1");
    var permissionCacheService = cacheScope.ServiceProvider.GetRequiredService<IAdminPermissionCacheService>();
    await permissionCacheService.RefreshAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Redis erişilemediği için access-control cache temizleme atlandı.");
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagCtx, httpContext) =>
    {
        if (httpContext.Items.TryGetValue("CorrelationId", out var corrId))
            diagCtx.Set("CorrelationId", corrId?.ToString() ?? string.Empty);

        diagCtx.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagCtx.Set("ClientIp",
            httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? string.Empty);
    };
});
app.UseCors("Default");
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
