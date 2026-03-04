using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum RewardStatus
{
    [Description("Yok")]
    None = 0,

    [Description("Beklemede")]
    Pending = 1,

    [Description("Onaylandı")]
    Approved = 2,

    [Description("Reddedildi")]
    Rejected = 3
}
