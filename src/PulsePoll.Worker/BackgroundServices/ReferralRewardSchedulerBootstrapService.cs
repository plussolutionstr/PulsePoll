using PulsePoll.Worker.Services;

namespace PulsePoll.Worker.BackgroundServices;

public class ReferralRewardSchedulerBootstrapService(
    IReferralRewardJobScheduler scheduler,
    ILogger<ReferralRewardSchedulerBootstrapService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await scheduler.RefreshAsync(cancellationToken);
        logger.LogInformation("Referral reward scheduler bootstrap completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
