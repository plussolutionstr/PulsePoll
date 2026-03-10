using PulsePoll.Application.Interfaces;

namespace PulsePoll.Worker.Jobs;

public class AffiliateRewardReconciliationRecurringJob(
    IAffiliateRewardService affiliateRewardService,
    ILogger<AffiliateRewardReconciliationRecurringJob> logger)
{
    public async Task ExecuteAsync()
    {
        var grantedCount = await affiliateRewardService.ReconcilePendingAsync(actorId: 0);
        logger.LogInformation(
            "Nightly affiliate reward reconciliation completed: GrantedCount={GrantedCount}",
            grantedCount);
    }
}
