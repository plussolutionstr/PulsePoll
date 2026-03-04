namespace PulsePoll.Domain.Entities;

public class Referral : EntityBase
{
    public int ReferrerId { get; set; }
    public int ReferredSubjectId { get; set; }
    public DateTime ReferredAt { get; set; } = DateTime.UtcNow;
    public decimal? CommissionEarned { get; set; }
    public decimal? CommissionAmountTry { get; set; }
    public string? CommissionUnitCode { get; set; }
    public string? CommissionUnitLabel { get; set; }
    public decimal? CommissionUnitTryMultiplier { get; set; }
    public DateTime? CommissionGrantedAt { get; set; }

    public Subject Referrer { get; set; } = null!;
    public Subject ReferredSubject { get; set; } = null!;
}
