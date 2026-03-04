using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class SubjectAssignmentJob
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int AdminId { get; set; }
    public int RequestedCount { get; set; }
    public int AssignedCount { get; set; }
    public int SkippedCount { get; set; }
    public AssignmentJobStatus Status { get; set; } = AssignmentJobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public Project Project { get; set; } = null!;
}