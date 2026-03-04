using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class AppContentConfigRepository(AppDbContext db) : IAppContentConfigRepository
{
    public Task<AppContentConfig?> GetCurrentAsync()
        => db.AppContentConfigs
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task UpsertAsync(AppContentConfig config, int actorId = 0)
    {
        var current = await GetCurrentAsync();
        if (current is null)
        {
            config.SetCreated(actorId);
            db.AppContentConfigs.Add(config);
        }
        else
        {
            current.KvkkText = config.KvkkText;
            current.ContactTitle = config.ContactTitle;
            current.ContactBody = config.ContactBody;
            current.ContactEmail = config.ContactEmail;
            current.ContactPhone = config.ContactPhone;
            current.ContactWhatsapp = config.ContactWhatsapp;
            current.SetUpdated(actorId);
        }

        await db.SaveChangesAsync();
    }
}
