using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class ReferralRewardConfigRepository(AppDbContext db) : IReferralRewardConfigRepository
{
    public Task<ReferralRewardConfig?> GetCurrentAsync()
        => db.ReferralRewardConfigs.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).FirstOrDefaultAsync();

    public async Task UpsertAsync(ReferralRewardConfig config, int actorId = 0)
    {
        var current = await GetCurrentAsync();
        if (current is null)
        {
            config.SetCreated(actorId);
            db.ReferralRewardConfigs.Add(config);
        }
        else
        {
            current.IsActive = config.IsActive;
            current.RewardAmount = config.RewardAmount;
            current.TriggerType = config.TriggerType;
            current.ActiveDaysThreshold = config.ActiveDaysThreshold;
            current.SetUpdated(actorId);
        }

        await db.SaveChangesAsync();
    }
}
