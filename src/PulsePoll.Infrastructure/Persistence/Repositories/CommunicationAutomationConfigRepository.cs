using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class CommunicationAutomationConfigRepository(AppDbContext db) : ICommunicationAutomationConfigRepository
{
    public Task<CommunicationAutomationConfig?> GetCurrentAsync()
        => db.CommunicationAutomationConfigs
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task UpsertAsync(CommunicationAutomationConfig config, int actorId = 0)
    {
        var current = await GetCurrentAsync();
        if (current is null)
        {
            config.SetCreated(actorId);
            db.CommunicationAutomationConfigs.Add(config);
        }
        else
        {
            current.DailyRunTime = config.DailyRunTime;
            current.TimeZoneId = config.TimeZoneId;
            current.SetUpdated(actorId);
        }

        await db.SaveChangesAsync();
    }
}
