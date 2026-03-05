using CommunityToolkit.Mvvm.ComponentModel;
using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public partial class NotificationCenterService : ObservableObject
{
    private readonly INotificationApiClient _notificationApiClient;
    private bool _isLoaded;

    public NotificationCenterService(INotificationApiClient notificationApiClient)
    {
        _notificationApiClient = notificationApiClient;
    }

    [ObservableProperty] private IReadOnlyList<NotificationModel> _items = [];
    [ObservableProperty] private int _unreadCount;
    [ObservableProperty] private bool _isLoading;
    public bool HasUnread => UnreadCount > 0;

    public async Task LoadAsync(bool force = false, CancellationToken ct = default)
    {
        if (IsLoading)
            return;

        if (_isLoaded && !force)
            return;

        IsLoading = true;
        try
        {
            var notifications = await _notificationApiClient.GetNotificationsAsync(ct);
            SetItems(notifications);
        }
        finally
        {
            _isLoaded = true;
            IsLoading = false;
        }
    }

    public async Task MarkAllReadAsync(CancellationToken ct = default)
    {
        if (Items.Count == 0 || UnreadCount == 0)
            return;

        var previous = Items.ToList();
        var read = previous.Select(n => n with { IsRead = true }).ToList();
        SetItems(read);

        try
        {
            await _notificationApiClient.MarkAllReadAsync(ct);
        }
        catch
        {
            SetItems(previous);
            throw;
        }
    }

    public async Task MarkOneReadAsync(int notificationId, CancellationToken ct = default)
    {
        var previous = Items.ToList();
        var updated = previous.Select(n => n.Id == notificationId ? n with { IsRead = true } : n).ToList();
        SetItems(updated);

        try
        {
            await _notificationApiClient.MarkOneReadAsync(notificationId, ct);
        }
        catch
        {
            SetItems(previous);
            throw;
        }
    }

    public async Task DeleteAsync(int notificationId, CancellationToken ct = default)
    {
        var previous = Items.ToList();
        var updated = previous.Where(n => n.Id != notificationId).ToList();
        SetItems(updated);

        try
        {
            await _notificationApiClient.DeleteAsync(notificationId, ct);
        }
        catch
        {
            SetItems(previous);
            throw;
        }
    }

    private void SetItems(IReadOnlyList<NotificationModel> items)
    {
        Items = items;
        UnreadCount = items.Count(n => !n.IsRead);
        OnPropertyChanged(nameof(HasUnread));
    }
}
