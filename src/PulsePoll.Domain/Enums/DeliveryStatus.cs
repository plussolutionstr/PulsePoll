using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum DeliveryStatus
{
    [Description("Bekliyor")] Pending = 0,
    [Description("Gönderildi")] Sent = 1,
    [Description("Başarısız")] Failed = 2,
    [Description("Atlandı")] Skipped = 3
}
