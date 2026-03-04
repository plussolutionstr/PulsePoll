using System.ComponentModel;

namespace PulsePoll.Domain.Enums;

public enum ApprovalStatus
{
    [Description("Beklemede")] Pending = 1,
    [Description("Onaylandı")] Approved = 2,
    [Description("Reddedildi")] Rejected = 3
}
