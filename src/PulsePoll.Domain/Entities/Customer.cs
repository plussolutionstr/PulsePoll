using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class Customer : EntityBase
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ShortName { get; set; } = string.Empty;

    [Required, MaxLength(11)]
    public string TaxNumber { get; set; } = string.Empty;

    public int TaxOfficeId { get; set; }

    [Required, MaxLength(20)]
    public string Phone1 { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone2 { get; set; }

    [MaxLength(20)]
    public string? Mobile { get; set; }

    [Required, MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public int CityId { get; set; }

    public int DistrictId { get; set; }

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? LogoUrl { get; set; }

    // Navigation properties
    public TaxOffice TaxOffice { get; set; } = null!;
    public City City { get; set; } = null!;
    public District District { get; set; } = null!;
}
