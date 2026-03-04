using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class TaxOffice
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Code { get; set; }

    public int? CityId { get; set; }

    // Navigation properties
    public City? City { get; set; }
}
