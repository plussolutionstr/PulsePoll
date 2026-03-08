using PulsePoll.Application.Interfaces;

namespace PulsePoll.Worker.Jobs;

public class ReferralRewardReconciliationRecurringJob(
    IReferralRewardService referralRewardService,
    ILogger<ReferralRewardReconciliationRecurringJob> logger)
{
    public async Task ExecuteAsync()
    {
        var grantedCount = await referralRewardService.ReconcilePendingAsync(actorId: 0);
        logger.LogInformation(
            "Nightly referral reward reconciliation completed: GrantedCount={GrantedCount}",
            grantedCount);
    }
}
