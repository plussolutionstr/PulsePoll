using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class ProjectSurveyHelperEntry : EntityBase
{
    public int ProjectId { get; set; }

    [Required, MaxLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string HelpText { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
}
