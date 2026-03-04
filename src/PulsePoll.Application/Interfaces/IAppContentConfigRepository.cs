using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IAppContentConfigRepository
{
    Task<AppContentConfig?> GetCurrentAsync();
    Task UpsertAsync(AppContentConfig config, int actorId = 0);
}
