using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class PaymentBatchItem : EntityBase
{
    public int PaymentBatchId { get; set; }
    public int WithdrawalRequestId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public PaymentBatch PaymentBatch { get; set; } = null!;
    public WithdrawalRequest WithdrawalRequest { get; set; } = null!;
}
