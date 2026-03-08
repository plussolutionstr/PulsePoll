using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Infrastructure.Persistence.Configurations;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class NotificationDistributionConfigRepository(AppDbContext db) : INotificationDistributionConfigRepository
{
    private const int SingletonId = NotificationDistributionConfigConfiguration.SingletonId;

    public Task<NotificationDistributionConfig?> GetCurrentAsync()
        => db.NotificationDistributionConfigs
            .SingleOrDefaultAsync(x => x.Id == SingletonId);

    public async Task UpsertAsync(NotificationDistributionConfig config, int actorId = 0)
    {
        var current = await db.NotificationDistributionConfigs
            .SingleOrDefaultAsync(x => x.Id == SingletonId);

        if (current is null)
        {
            config.Id = SingletonId;
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
