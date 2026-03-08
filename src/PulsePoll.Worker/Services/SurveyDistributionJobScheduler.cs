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

    private static readonly TimeZoneInfo IstanbulTz = GetIstanbulTimeZone();

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        // Her saat başı 09:00-19:00 arası (Türkiye saati)
        recurringJobManager.AddOrUpdate<SurveyDistributionRecurringJob>(
            HourlyJobId,
            job => job.ExecuteAsync(),
            "0 9-19 * * *",
            new RecurringJobOptions { TimeZone = IstanbulTz });

        // Her gün 10:00'da hatırlatma (Türkiye saati)
        recurringJobManager.AddOrUpdate<SurveyReminderRecurringJob>(
            DailyReminderJobId,
            job => job.ExecuteAsync(),
            "0 10 * * *",
            new RecurringJobOptions { TimeZone = IstanbulTz });

        logger.LogInformation("Survey distribution jobs scheduled. Hourly: 09-19, Reminder: 10:00 Istanbul time.");
        return Task.CompletedTask;
    }

    private static TimeZoneInfo GetIstanbulTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        catch { return TimeZoneInfo.Local; }
    }
}
