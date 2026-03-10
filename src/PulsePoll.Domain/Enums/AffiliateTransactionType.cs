using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum AffiliateTransactionType
{
    [Description("Komisyon")]
    Commission = 0,

    [Description("Ödeme")]
    Payment = 1,

    [Description("Artı (+)")]
    Credit = 2,

    [Description("Eksi (-)")]
    Debit = 3
}
