using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum WalletTransactionType
{
    [Description("Giriş (Alacak)")]
    Credit = 0,
    
    [Description("Çıkış (Borç)")]
    Withdrawal = 1
}
