using System.ComponentModel.DataAnnotations;

namespace PulsePoll.Domain.Entities;

public class ExternalAffiliate : EntityBase
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(34)]
    public string? Iban { get; set; }

    [Required, MaxLength(20)]
    public string AffiliateCode { get; set; } = string.Empty;

    public decimal? CommissionAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal Balance { get; set; }

    public decimal TotalEarned { get; set; }

    public decimal TotalPaid { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }

    public uint Version { get; set; }

    // Navigation properties
    public ICollection<AffiliateTransaction> Transactions { get; set; } = [];
    public ICollection<Subject> ReferredSubjects { get; set; } = [];
}
