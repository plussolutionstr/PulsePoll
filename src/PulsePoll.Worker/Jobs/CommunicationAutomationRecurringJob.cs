using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Worker.Jobs;

public class CommunicationAutomationRecurringJob(
    ICommunicationAutomationConfigService communicationAutomationConfigService,
    IMessageAutomationService messageAutomationService,
    ISpecialDayCalendarService specialDayCalendarService,
    ILogger<CommunicationAutomationRecurringJob> logger)
{
    public async Task ExecuteAsync()
    {
        var config = await communicationAutomationConfigService.GetAsync();
        var timeZone = ResolveTimeZone(config.TimeZoneId);
        var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).DateTime;
        var today = DateOnly.FromDateTime(nowLocal);

        await specialDayCalendarService.SyncYearAsync(nowLocal.Year, adminId: 0);
        await specialDayCalendarService.SyncYearAsync(nowLocal.Year + 1, adminId: 0);

        var birthdayResults = await messageAutomationService.RunDueCampaignsAsync(
            today,
            MessageTriggerType.Birthday,
            triggerKey: null,
            adminId: 0);

        var eventCodes = await specialDayCalendarService.GetEventCodesByDateAsync(today);
        foreach (var code in eventCodes)
        {
            await messageAutomationService.RunDueCampaignsAsync(
                today,
                MessageTriggerType.SpecialDay,
                triggerKey: code,
                adminId: 0);
        }

        logger.LogInformation(
            "Hangfire communication automation completed: Date={Date} BirthdayCampaignCount={BirthdayCount} SpecialDayEventCount={SpecialDayEvents}",
            today, birthdayResults.Count, eventCodes.Count);
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception)
        {
            return TimeZoneInfo.Local;
        }
    }
}
