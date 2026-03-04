using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class LSMSocioeconomicStatus
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Code { get; set; }
}
