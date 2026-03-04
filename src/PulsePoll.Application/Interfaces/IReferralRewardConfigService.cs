using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IReferralRewardConfigService
{
    Task<ReferralRewardConfigDto> GetAsync();
    Task SaveAsync(ReferralRewardConfigDto dto, int adminId);
}
