using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IRegistrationConfigRepository
{
    Task<RegistrationConfig?> GetCurrentAsync();
    Task UpsertAsync(RegistrationConfig config, int actorId = 0);
}
