using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class RewardUnitConfigService(IRewardUnitConfigRepository repository) : IRewardUnitConfigService
{
    public async Task<RewardUnitConfigDto> GetAsync()
    {
        var current = await repository.GetCurrentAsync();
        return current is null ? Default() : ToDto(current);
    }

    public async Task SaveAsync(RewardUnitConfigDto dto, int adminId)
    {
        Validate(dto);

        var entity = new RewardUnitConfig
        {
            UnitCode = dto.UnitCode.Trim().ToUpperInvariant(),
            UnitLabel = dto.UnitLabel.Trim(),
            TryMultiplier = dto.TryMultiplier
        };

        await repository.UpsertAsync(entity, adminId);
    }

    public async Task<decimal> ConvertToTryAsync(decimal rewardAmount)
    {
        var config = await GetAsync();
        return decimal.Round(rewardAmount * config.TryMultiplier, 2, MidpointRounding.AwayFromZero);
    }

    public static RewardUnitConfigDto Default()
        => new(
            UnitCode: "TRY",
            UnitLabel: "TL",
            TryMultiplier: 1m);

    private static RewardUnitConfigDto ToDto(RewardUnitConfig x)
        => new(
            x.UnitCode,
            x.UnitLabel,
            x.TryMultiplier);

    private static void Validate(RewardUnitConfigDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UnitCode))
            throw new BusinessException("INVALID_UNIT_CODE", "Birim kodu zorunludur.");

        if (dto.UnitCode.Length is < 2 or > 16)
            throw new BusinessException("INVALID_UNIT_CODE", "Birim kodu 2-16 karakter olmalıdır.");

        if (string.IsNullOrWhiteSpace(dto.UnitLabel))
            throw new BusinessException("INVALID_UNIT_LABEL", "Birim etiketi zorunludur.");

        if (dto.UnitLabel.Length is < 1 or > 20)
            throw new BusinessException("INVALID_UNIT_LABEL", "Birim etiketi 1-20 karakter olmalıdır.");

        if (dto.TryMultiplier <= 0)
            throw new BusinessException("INVALID_TRY_MULTIPLIER", "TL çarpanı 0'dan büyük olmalıdır.");
    }
}
