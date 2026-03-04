using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IRewardUnitConfigRepository
{
    Task<RewardUnitConfig?> GetCurrentAsync();
    Task UpsertAsync(RewardUnitConfig config, int actorId = 0);
}
