using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class EducationLevel
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int OrderIndex { get; set; }
}
