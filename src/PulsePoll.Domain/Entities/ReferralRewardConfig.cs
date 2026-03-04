using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class ReferralRewardConfig : EntityBase
{
    public bool IsActive { get; set; } = true;
    public decimal RewardAmount { get; set; } = 10m;
    public ReferralRewardTriggerType TriggerType { get; set; } = ReferralRewardTriggerType.FirstRewardApproved;
    public int ActiveDaysThreshold { get; set; } = 7;
}
