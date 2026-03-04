namespace PulsePoll.Domain.Entities;

public class BankAccount : EntityBase
{
    public int SubjectId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string IbanLast4 { get; set; } = string.Empty;
    public string IbanEncrypted { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public Subject Subject { get; set; } = null!;
}
