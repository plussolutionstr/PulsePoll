using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public interface INotificationApiClient
{
    Task<List<NotificationModel>> GetNotificationsAsync(CancellationToken ct = default);
    Task MarkAllReadAsync(CancellationToken ct = default);
    Task MarkOneReadAsync(int notificationId, CancellationToken ct = default);
    Task DeleteAsync(int notificationId, CancellationToken ct = default);
}
