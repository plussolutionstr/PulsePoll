using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum PaymentBatchStatus
{
    [Description("Taslak")]    Draft     = 1,
    [Description("Gönderildi")] Sent     = 2,
    [Description("Tamamlandı")] Completed = 3
}
