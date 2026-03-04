using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class WalletTransaction : EntityBase
{
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
    public WalletTransactionType Type { get; set; }
    public string? Description { get; set; }
    public string? ReferenceId { get; set; }

    public Wallet Wallet { get; set; } = null!;
}
