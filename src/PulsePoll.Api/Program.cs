using PulsePoll.Api.Extensions;
using PulsePoll.Api.Logging;
using PulsePoll.Api.Middleware;
using PulsePoll.Infrastructure;
using PulsePoll.Infrastructure.Persistence;
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
app.UseCors("Dev");
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
