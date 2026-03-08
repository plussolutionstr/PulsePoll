using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class NotificationDistributionConfigRepository(AppDbContext db) : INotificationDistributionConfigRepository
{
    public Task<NotificationDistributionConfig?> GetCurrentAsync()
        => db.NotificationDistributionConfigs
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task UpsertAsync(NotificationDistributionConfig config, int actorId = 0)
    {
        var current = await GetCurrentAsync();
        if (current is null)
        {
            config.SetCreated(actorId);
            db.NotificationDistributionConfigs.Add(config);
        }
        else
        {
            current.HourlyLimit = config.HourlyLimit;
            current.SetUpdated(actorId);
        }

        await db.SaveChangesAsync();
    }
}
