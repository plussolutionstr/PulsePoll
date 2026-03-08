using PulsePoll.Worker.Services;

namespace PulsePoll.Worker.BackgroundServices;

public class SurveyDistributionSchedulerBootstrapService(
    ISurveyDistributionJobScheduler scheduler,
    ILogger<SurveyDistributionSchedulerBootstrapService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await scheduler.RefreshAsync(cancellationToken);
        logger.LogInformation("Survey distribution scheduler bootstrap tamamlandı.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
