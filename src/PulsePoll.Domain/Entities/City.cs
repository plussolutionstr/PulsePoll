using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class City
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int PlateCode { get; set; }

    [MaxLength(50)]
    public string? Region { get; set; }

    [MaxLength(10)]
    public string? Nuts1Code { get; set; }

    [MaxLength(100)]
    public string? Nuts1Region { get; set; }

    public ICollection<District> Districts { get; set; } = [];
}
