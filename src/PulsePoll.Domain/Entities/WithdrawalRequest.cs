using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class WithdrawalRequest : EntityBase
{
    public int SubjectId { get; set; }
    public int BankAccountId { get; set; }
    public int WalletTransactionId { get; set; }
    // Subject reward unit amount (e.g., 1000 Poll)
    public decimal Amount { get; set; }
    // Snapshot of unit conversion at request time (e.g., 1 Poll = 0.10 TL)
    public decimal AmountTry { get; set; }
    public string UnitCode { get; set; } = "TRY";
    public string UnitLabel { get; set; } = "TL";
    public decimal UnitTryMultiplier { get; set; } = 1m;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedBy { get; set; }

    public Subject Subject { get; set; } = null!;
    public BankAccount BankAccount { get; set; } = null!;
    public WalletTransaction WalletTransaction { get; set; } = null!;
}
