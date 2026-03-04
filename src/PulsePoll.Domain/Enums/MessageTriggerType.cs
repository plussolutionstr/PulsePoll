using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum MessageTriggerType
{
    [Description("Özel Gün")] SpecialDay = 1,
    [Description("Doğum Günü")] Birthday = 2
}
