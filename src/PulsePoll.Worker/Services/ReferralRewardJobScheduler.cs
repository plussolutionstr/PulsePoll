using Hangfire;
using PulsePoll.Worker.Jobs;

namespace PulsePoll.Worker.Services;

public class ReferralRewardJobScheduler(
    IRecurringJobManager recurringJobManager,
    ILogger<ReferralRewardJobScheduler> logger) : IReferralRewardJobScheduler
{
    public const string ReconciliationJobId = "referral-reward-reconciliation-nightly";

    private static readonly TimeZoneInfo IstanbulTz = ResolveTurkeyTimeZone();

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        recurringJobManager.AddOrUpdate<ReferralRewardReconciliationRecurringJob>(
            ReconciliationJobId,
            job => job.ExecuteAsync(),
            "0 3 * * *",
            new RecurringJobOptions { TimeZone = IstanbulTz });

        logger.LogInformation(
            "Referral reward reconciliation scheduled: Cron={Cron} TimeZone={TimeZoneId}",
            "0 3 * * *",
            IstanbulTz.Id);

        return Task.CompletedTask;
    }

    private static TimeZoneInfo ResolveTurkeyTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
    }
}
