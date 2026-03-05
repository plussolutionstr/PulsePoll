using CommunityToolkit.Mvvm.ComponentModel;
using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public partial class NotificationCenterService : ObservableObject
{
    private readonly INotificationApiClient _notificationApiClient;
    private readonly MockDataService _mockDataService;
    private bool _isLoaded;
    private bool _usingFallback;

    public NotificationCenterService(INotificationApiClient notificationApiClient, MockDataService mockDataService)
    {
        _notificationApiClient = notificationApiClient;
        _mockDataService = mockDataService;
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
            _usingFallback = false;
            SetItems(notifications);
        }
        catch
        {
            _usingFallback = true;
            SetItems(_mockDataService.GetNotifications().OrderByDescending(n => n.Date).ToList());
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

        if (_usingFallback)
            return;

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

    private void SetItems(IReadOnlyList<NotificationModel> items)
    {
        Items = items;
        UnreadCount = items.Count(n => !n.IsRead);
        OnPropertyChanged(nameof(HasUnread));
    }
}
