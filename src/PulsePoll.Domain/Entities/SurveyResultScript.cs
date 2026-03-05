using System.ComponentModel.DataAnnotations;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class SurveyResultScript : EntityBase
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<SurveyResultPattern> Patterns { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}

public class SurveyResultPattern : EntityBase
{
    public int SurveyResultScriptId { get; set; }

    public AssignmentStatus Status { get; set; }

    [Required, MaxLength(500)]
    public string MatchPattern { get; set; } = string.Empty;

    public int Order { get; set; } = 0;

    public SurveyResultScript SurveyResultScript { get; set; } = null!;
}
