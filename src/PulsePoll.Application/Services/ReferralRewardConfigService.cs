using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class ReferralRewardConfigService(IReferralRewardConfigRepository repository) : IReferralRewardConfigService
{
    public async Task<ReferralRewardConfigDto> GetAsync()
    {
        var current = await repository.GetCurrentAsync();
        return current is null ? Default() : ToDto(current);
    }

    public async Task SaveAsync(ReferralRewardConfigDto dto, int adminId)
    {
        Validate(dto);

        var entity = new ReferralRewardConfig
        {
            IsActive = dto.IsActive,
            RewardAmount = dto.RewardAmount,
            TriggerType = dto.TriggerType,
            ActiveDaysThreshold = dto.ActiveDaysThreshold
        };

        await repository.UpsertAsync(entity, adminId);
    }

    public static ReferralRewardConfigDto Default()
        => new(
            IsActive: true,
            RewardAmount: 10m,
            TriggerType: ReferralRewardTriggerType.FirstRewardApproved,
            ActiveDaysThreshold: 7);

    private static ReferralRewardConfigDto ToDto(ReferralRewardConfig x)
        => new(
            x.IsActive,
            x.RewardAmount,
            x.TriggerType,
            x.ActiveDaysThreshold);

    private static void Validate(ReferralRewardConfigDto dto)
    {
        if (dto.RewardAmount < 0)
            throw new BusinessException("INVALID_REFERRAL_REWARD", "Referans ödülü negatif olamaz.");

        if (dto.ActiveDaysThreshold <= 0)
            throw new BusinessException("INVALID_ACTIVE_DAYS_THRESHOLD", "Aktif gün eşiği 0'dan büyük olmalıdır.");

        if (dto.ActiveDaysThreshold > 3650)
            throw new BusinessException("INVALID_ACTIVE_DAYS_THRESHOLD", "Aktif gün eşiği çok yüksek.");
    }
}
