using PulsePoll.Worker.Services;

namespace PulsePoll.Worker.BackgroundServices;

public class CommunicationAutomationSchedulerBootstrapService(
    ICommunicationAutomationJobScheduler scheduler,
    ILogger<CommunicationAutomationSchedulerBootstrapService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await scheduler.RefreshAsync(cancellationToken);
        logger.LogInformation("Communication automation schedule bootstrap completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
