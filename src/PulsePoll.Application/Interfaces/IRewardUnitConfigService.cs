using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IRewardUnitConfigService
{
    Task<RewardUnitConfigDto> GetAsync();
    Task SaveAsync(RewardUnitConfigDto dto, int adminId);
    Task<decimal> ConvertToTryAsync(decimal rewardAmount);
}
