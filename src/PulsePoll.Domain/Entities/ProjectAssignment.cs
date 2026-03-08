using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class ProjectAssignment : EntityBase
{
    public int ProjectId { get; set; }
    public int SubjectId { get; set; }
    public DateTime AssignedAt { get; set; } = TurkeyTime.Now;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.NotStarted;
    public DateTime? CompletedAt { get; set; }
    public decimal? EarnedAmount { get; set; }
    public RewardStatus RewardStatus { get; set; } = RewardStatus.None;
    public DateTime? RewardProcessedAt { get; set; }
    public int? RewardProcessedBy { get; set; }
    public string? RewardRejectionReason { get; set; }
    public DateTime? ScheduledNotifiedAt { get; set; }

    public Project Project { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
