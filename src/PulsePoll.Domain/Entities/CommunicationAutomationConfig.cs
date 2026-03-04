namespace PulsePoll.Domain.Entities;

public class CommunicationAutomationConfig : EntityBase
{
    public string DailyRunTime { get; set; } = "09:00";
    public string TimeZoneId { get; set; } = "Europe/Istanbul";
}
