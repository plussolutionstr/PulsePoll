using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class RewardUnitConfigRepository(AppDbContext db) : IRewardUnitConfigRepository
{
    public Task<RewardUnitConfig?> GetCurrentAsync()
        => db.RewardUnitConfigs.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).FirstOrDefaultAsync();

    public async Task UpsertAsync(RewardUnitConfig config, int actorId = 0)
    {
        var current = await GetCurrentAsync();
        if (current is null)
        {
            config.SetCreated(actorId);
            db.RewardUnitConfigs.Add(config);
        }
        else
        {
            current.UnitCode = config.UnitCode;
            current.UnitLabel = config.UnitLabel;
            current.TryMultiplier = config.TryMultiplier;
            current.SetUpdated(actorId);
        }

        await db.SaveChangesAsync();
    }
}
