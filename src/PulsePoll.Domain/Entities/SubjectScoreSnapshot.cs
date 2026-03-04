namespace PulsePoll.Domain.Entities;

public class SubjectScoreSnapshot : EntityBase
{
    public int SubjectId { get; set; }

    public decimal Score { get; set; }
    public int Star { get; set; }
    public decimal CoreScore { get; set; }
    public decimal ActivityMultiplier { get; set; } = 1m;

    public int TotalAssignments { get; set; }
    public int Started { get; set; }
    public int Completed { get; set; }
    public int NotStarted { get; set; }
    public int Partial { get; set; }
    public int Disqualify { get; set; }
    public int ScreenOut { get; set; }
    public int QuotaFull { get; set; }
    public int RewardApproved { get; set; }
    public int RewardRejected { get; set; }
    public int? MedianCompletionMinutes { get; set; }

    public int? ActiveDays30 { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public Subject Subject { get; set; } = null!;
}

