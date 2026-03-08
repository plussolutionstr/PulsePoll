using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface INotificationDistributionConfigRepository
{
    Task<NotificationDistributionConfig?> GetCurrentAsync();
    Task UpsertAsync(NotificationDistributionConfig config, int actorId = 0);
}
