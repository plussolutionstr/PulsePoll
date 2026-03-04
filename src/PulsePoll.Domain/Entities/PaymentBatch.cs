using System.ComponentModel.DataAnnotations;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class PaymentBatch : EntityBase
{
    [Required, MaxLength(30)]
    public string BatchNumber { get; set; } = string.Empty;

    public PaymentBatchStatus Status { get; set; } = PaymentBatchStatus.Draft;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string? Note { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<PaymentBatchItem> Items { get; set; } = [];
}
