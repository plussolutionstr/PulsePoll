using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum PaymentStatus
{
    [Description("Beklemede")] Pending = 1,
    [Description("Ödendi")]    Paid    = 2,
    [Description("Başarısız")] Failed  = 3
}
