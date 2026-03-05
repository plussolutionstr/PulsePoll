using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public interface INotificationApiClient
{
    Task<List<NotificationModel>> GetNotificationsAsync(CancellationToken ct = default);
    Task MarkAllReadAsync(CancellationToken ct = default);
}
