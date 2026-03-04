using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record ReferralRewardConfigDto(
    bool IsActive,
    decimal RewardAmount,
    ReferralRewardTriggerType TriggerType,
    int ActiveDaysThreshold);
