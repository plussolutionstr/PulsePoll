using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IReferralRewardConfigRepository
{
    Task<ReferralRewardConfig?> GetCurrentAsync();
    Task UpsertAsync(ReferralRewardConfig config, int actorId = 0);
}
