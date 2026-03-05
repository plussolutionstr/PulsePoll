using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class Project : EntityBase
{
    public int CustomerId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public int ParticipantCount { get; set; }

    public int TotalTargetCount { get; set; }

    public int DurationDays { get; set; }

    public DateOnly? StartDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Budget { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Reward { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ConsolationReward { get; set; }

    [Required, MaxLength(2000)]
    public string SurveyUrl { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string SubjectParameterName { get; set; } = string.Empty;

    public int EstimatedMinutes { get; set; }

    [MaxLength(2000)]
    public string? CustomerBriefing { get; set; }

    [Required, MaxLength(500)]
    public string StartMessage { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string CompletedMessage { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string DisqualifyMessage { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string QuotaFullMessage { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ScreenOutMessage { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    // StartDate + DurationDays'den hesaplanır, DB'ye yazılmaz
    [NotMapped]
    public DateOnly? EndDate => StartDate.HasValue ? StartDate.Value.AddDays(DurationDays) : null;

    public int? CoverMediaId { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public MediaAsset? CoverMedia { get; set; }
    public ICollection<ProjectAssignment> Assignments { get; set; } = [];
}
