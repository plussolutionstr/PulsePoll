namespace PulsePoll.Domain.Entities;

public class Wallet : EntityBase
{
    public int SubjectId { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalEarned { get; set; }
    public uint Version { get; set; }

    public Subject Subject { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = [];
}
