using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class SocioeconomicStatus
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Code { get; set; }

    public decimal? MinIncome { get; set; }

    public decimal? MaxIncome { get; set; }

    public int SortOrder { get; set; }
}
