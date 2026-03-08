using Hangfire;
using PulsePoll.Worker.Jobs;

namespace PulsePoll.Worker.Services;

public interface ISurveyDistributionJobScheduler
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

public class SurveyDistributionJobScheduler(
    IRecurringJobManager recurringJobManager,
    ILogger<SurveyDistributionJobScheduler> logger) : ISurveyDistributionJobScheduler
{
    public const string HourlyJobId = "survey-distribution-hourly";
    public const string DailyReminderJobId = "survey-distribution-reminder";

    private static readonly TimeZoneInfo IstanbulTz = ResolveTurkeyTimeZone();

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        // Her saat başı kontrol et; proje bazlı pencere filtresi servis katmanında uygulanır.
        recurringJobManager.AddOrUpdate<SurveyDistributionRecurringJob>(
            HourlyJobId,
            job => job.ExecuteAsync(),
            "0 * * * *",
            new RecurringJobOptions { TimeZone = IstanbulTz });

        // Her saat başı kontrol et; hatırlatma yalnızca proje başlangıç saatinde gönderilir.
        recurringJobManager.AddOrUpdate<SurveyReminderRecurringJob>(
            DailyReminderJobId,
            job => job.ExecuteAsync(),
            "0 * * * *",
            new RecurringJobOptions { TimeZone = IstanbulTz });

        logger.LogInformation("Survey distribution jobs scheduled. Distribution=hourly Reminder=hourly TimeZone={TimeZoneId}", IstanbulTz.Id);
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
