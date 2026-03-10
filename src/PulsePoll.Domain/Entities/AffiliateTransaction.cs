using System.ComponentModel.DataAnnotations;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class AffiliateTransaction : EntityBase
{
    public int ExternalAffiliateId { get; set; }

    public AffiliateTransactionType Type { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? SubjectId { get; set; }

    [MaxLength(100)]
    public string? ReferenceId { get; set; }

    // Navigation properties
    public ExternalAffiliate ExternalAffiliate { get; set; } = null!;
    public Subject? Subject { get; set; }
}
