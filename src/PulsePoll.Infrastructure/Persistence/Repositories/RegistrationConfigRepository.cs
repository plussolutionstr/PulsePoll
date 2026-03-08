using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Infrastructure.Persistence.Configurations;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class RegistrationConfigRepository(AppDbContext db) : IRegistrationConfigRepository
{
    private const int SingletonId = RegistrationConfigConfiguration.SingletonId;

    public Task<RegistrationConfig?> GetCurrentAsync()
        => db.RegistrationConfigs
            .SingleOrDefaultAsync(x => x.Id == SingletonId);

    public async Task UpsertAsync(RegistrationConfig config, int actorId = 0)
    {
        var current = await db.RegistrationConfigs
            .SingleOrDefaultAsync(x => x.Id == SingletonId);

        if (current is null)
        {
            config.Id = SingletonId;
            config.SetCreated(actorId);
            db.RegistrationConfigs.Add(config);
        }
        else
        {
            current.AutoApproveNewSubjects = config.AutoApproveNewSubjects;
            current.SetUpdated(actorId);
        }

        await db.SaveChangesAsync();
    }
}
