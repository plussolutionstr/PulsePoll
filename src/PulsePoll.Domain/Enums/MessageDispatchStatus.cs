using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum MessageDispatchStatus
{
    [Description("Sıraya Alındı")] Queued = 1,
    [Description("Atlandı")] Skipped = 2,
    [Description("Hata")] Failed = 3
}
