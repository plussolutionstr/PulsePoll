using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly NotificationCenterService _notificationCenter;

    public NotificationsViewModel(NotificationCenterService notificationCenter)
    {
        _notificationCenter = notificationCenter;
        _notificationCenter.PropertyChanged += OnNotificationCenterPropertyChanged;
        ApplyFilter();
    }

    [ObservableProperty] private ObservableCollection<NotificationModel> _notifications = [];
    [ObservableProperty] private string _selectedFilter = "Tümü";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _hasNotifications;
    [ObservableProperty] private bool _canMarkAllRead;
    [ObservableProperty] private bool _showEmptyState;

    public List<string> Filters => ["Tümü", "Anketler", "Kazançlar", "Sistem"];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        try
        {
            await _notificationCenter.LoadAsync(force: true);
            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task MarkAllReadAsync()
    {
        if (!CanMarkAllRead)
            return;

        try
        {
            await _notificationCenter.MarkAllReadAsync();
            ApplyFilter();
        }
        catch
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync("Hata", "Bildirimler işaretlenemedi.", "Tamam");
            }
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    // Popup state
    [ObservableProperty] private NotificationModel? _selectedNotification;
    [ObservableProperty] private bool _isPopupVisible;

    [RelayCommand]
    private void ShowNotification(NotificationModel notification)
    {
        SelectedNotification = notification;
        IsPopupVisible = true;
        if (!notification.IsRead)
            _ = MarkOneReadAsync(notification);
    }

    [RelayCommand]
    private void ClosePopup()
    {
        IsPopupVisible = false;
        SelectedNotification = null;
    }

    [RelayCommand]
    private async Task MarkOneReadAsync(NotificationModel notification)
    {
        try
        {
            await _notificationCenter.MarkOneReadAsync(notification.Id);
        }
        catch { }
    }

    [RelayCommand]
    private async Task DeleteNotificationAsync(NotificationModel notification)
    {
        try
        {
            await _notificationCenter.DeleteAsync(notification.Id);
        }
        catch
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlertAsync("Hata", "Bildirim silinemedi.", "Tamam");
        }
    }

    private void ApplyFilter()
    {
        var items = _notificationCenter.Items.AsEnumerable();

        items = SelectedFilter switch
        {
            "Anketler" => items.Where(n => n.Type is "survey" or "disqualified"),
            "Kazançlar" => items.Where(n => n.Type is "earning" or "rank"),
            "Sistem" => items.Where(n => n.Type == "system"),
            _ => items
        };

        Notifications = new ObservableCollection<NotificationModel>(items.OrderByDescending(n => n.Date));
        HasNotifications = Notifications.Count > 0;
        CanMarkAllRead = _notificationCenter.HasUnread;
        ShowEmptyState = !IsLoading && !HasNotifications;
    }

    partial void OnIsLoadingChanged(bool value)
    {
        ShowEmptyState = !value && !HasNotifications;
    }

    private void OnNotificationCenterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NotificationCenterService.Items) or nameof(NotificationCenterService.UnreadCount))
            ApplyFilter();
    }
}
