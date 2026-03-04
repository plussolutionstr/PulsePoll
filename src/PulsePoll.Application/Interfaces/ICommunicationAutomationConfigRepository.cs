using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ICommunicationAutomationConfigRepository
{
    Task<CommunicationAutomationConfig?> GetCurrentAsync();
    Task UpsertAsync(CommunicationAutomationConfig config, int actorId = 0);
}
