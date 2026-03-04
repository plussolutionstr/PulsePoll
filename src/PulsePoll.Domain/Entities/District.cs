using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class District
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int CityId { get; set; }

    public City City { get; set; } = null!;
}
