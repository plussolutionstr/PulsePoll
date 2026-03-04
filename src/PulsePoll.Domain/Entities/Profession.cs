using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class Profession
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Code { get; set; }
}
