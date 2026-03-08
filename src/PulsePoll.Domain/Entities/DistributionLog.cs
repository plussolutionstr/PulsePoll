namespace PulsePoll.Domain.Entities;

public class DistributionLog : EntityBase
{
    public int ProjectId { get; set; }
    public DateOnly RunDate { get; set; }
    public TimeOnly RunTime { get; set; }
    public int ScheduledBefore { get; set; }
    public int DistributedCount { get; set; }
    public int DailyQuota { get; set; }
    public int HourlyQuota { get; set; }
    public int RemainingDays { get; set; }
    public bool IsLastDayFlush { get; set; }

    public Project Project { get; set; } = null!;
}
