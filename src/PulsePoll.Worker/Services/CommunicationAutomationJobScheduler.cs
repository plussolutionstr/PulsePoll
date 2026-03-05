using System.Globalization;
using Hangfire;
using PulsePoll.Application.Interfaces;
using PulsePoll.Worker.Jobs;

namespace PulsePoll.Worker.Services;

public class CommunicationAutomationJobScheduler(
    ICommunicationAutomationConfigService communicationAutomationConfigService,
    IRecurringJobManager recurringJobManager,
    ILogger<CommunicationAutomationJobScheduler> logger) : ICommunicationAutomationJobScheduler
{
    public const string DailyJobId = "communication-automation-daily";
    private static readonly TimeOnly DefaultRunTime = new(9, 0);

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        var config = await communicationAutomationConfigService.GetAsync();
        var runTime = ParseRunTime(config.DailyRunTime);
        var timeZone = ResolveTimeZone(config.TimeZoneId);
        var cronExpression = $"{runTime.Minute} {runTime.Hour} * * *";

        recurringJobManager.AddOrUpdate<CommunicationAutomationRecurringJob>(
            DailyJobId,
            job => job.ExecuteAsync(),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = timeZone
            });

        logger.LogInformation(
            "Communication automation recurring job scheduled. DailyRunTime={DailyRunTime} TimeZone={TimeZone} Cron={Cron}",
            config.DailyRunTime,
            timeZone.Id,
            cronExpression);
    }

    private static TimeOnly ParseRunTime(string value)
    {
        if (TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;

        return DefaultRunTime;
    }

    private TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException ex)
        {
            logger.LogWarning(ex, "Configured timezone not found: {TimeZoneId}. Falling back to local timezone.", timeZoneId);
            return TimeZoneInfo.Local;
        }
        catch (InvalidTimeZoneException ex)
        {
            logger.LogWarning(ex, "Configured timezone is invalid: {TimeZoneId}. Falling back to local timezone.", timeZoneId);
            return TimeZoneInfo.Local;
        }
    }
}
