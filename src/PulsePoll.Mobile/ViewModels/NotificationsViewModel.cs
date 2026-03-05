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

    [ObservableProperty] private ObservableCollection<SelectableNotification> _notifications = [];
    [ObservableProperty] private string _selectedFilter = "Tümü";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _hasNotifications;
    [ObservableProperty] private bool _showEmptyState;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private bool _allSelected;

    public bool HasSelection => SelectedCount > 0;
    public string EditButtonText => IsEditMode ? "Bitti" : "Düzenle";
    public string SelectAllButtonText => AllSelected ? "Seçimi Temizle" : "Tümünü Seç";

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
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
        if (!IsEditMode)
            ClearSelection();
        OnPropertyChanged(nameof(EditButtonText));
    }

    [RelayCommand]
    private void ToggleSelection(SelectableNotification item)
    {
        if (!IsEditMode) return;
        item.IsSelected = !item.IsSelected;
        UpdateSelectionState();
    }

    [RelayCommand]
    private void ToggleSelectAll()
    {
        if (AllSelected)
        {
            foreach (var n in Notifications)
                n.IsSelected = false;
        }
        else
        {
            foreach (var n in Notifications)
                n.IsSelected = true;
        }

        UpdateSelectionState();
    }

    [RelayCommand]
    private async Task MarkSelectedReadAsync()
    {
        var selected = Notifications.Where(n => n.IsSelected).Select(n => n.Model).ToList();
        if (selected.Count == 0) return;

        var failCount = 0;
        foreach (var notification in selected)
        {
            if (notification.IsRead) continue;
            try
            {
                await _notificationCenter.MarkOneReadAsync(notification.Id);
            }
            catch
            {
                failCount++;
            }
        }

        if (failCount > 0 && Shell.Current is not null)
            await Shell.Current.DisplayAlertAsync("Hata", $"{failCount} bildirim işaretlenemedi.", "Tamam");

        IsEditMode = false;
        OnPropertyChanged(nameof(EditButtonText));
        ClearSelection();
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        var selected = Notifications.Where(n => n.IsSelected).Select(n => n.Model).ToList();
        if (selected.Count == 0) return;

        if (Shell.Current is null) return;

        var message = selected.Count == 1
            ? "Seçili bildirim silinecek."
            : $"{selected.Count} bildirim silinecek.";

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Bildirimleri Sil",
            message,
            "Sil",
            "Vazgeç");

        if (!confirmed) return;

        var failCount = 0;
        foreach (var notification in selected)
        {
            try
            {
                await _notificationCenter.DeleteAsync(notification.Id);
            }
            catch
            {
                failCount++;
            }
        }

        if (failCount > 0 && Shell.Current is not null)
            await Shell.Current.DisplayAlertAsync("Hata", $"{failCount} bildirim silinemedi.", "Tamam");

        IsEditMode = false;
        OnPropertyChanged(nameof(EditButtonText));
        ClearSelection();
    }

    // Popup state
    [ObservableProperty] private NotificationModel? _selectedNotification;
    [ObservableProperty] private bool _isPopupVisible;

    [RelayCommand]
    private void ShowNotification(SelectableNotification item)
    {
        if (IsEditMode)
        {
            ToggleSelection(item);
            return;
        }

        SelectedNotification = item.Model;
        IsPopupVisible = true;
        if (!item.Model.IsRead)
            _ = MarkOneReadInternalAsync(item.Model);
    }

    [RelayCommand]
    private void ClosePopup()
    {
        IsPopupVisible = false;
        SelectedNotification = null;
    }

    private async Task MarkOneReadInternalAsync(NotificationModel notification)
    {
        try
        {
            await _notificationCenter.MarkOneReadAsync(notification.Id);
        }
        catch { }
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

        Notifications = new ObservableCollection<SelectableNotification>(
            items.OrderByDescending(n => n.Date).Select(n => new SelectableNotification(n)));
        HasNotifications = Notifications.Count > 0;
        ShowEmptyState = !IsLoading && !HasNotifications;
        SelectedCount = 0;
        AllSelected = false;
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(SelectAllButtonText));
    }

    private void ClearSelection()
    {
        foreach (var n in Notifications)
            n.IsSelected = false;
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        SelectedCount = Notifications.Count(n => n.IsSelected);
        AllSelected = Notifications.Count > 0 && SelectedCount == Notifications.Count;
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(SelectAllButtonText));
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

public partial class SelectableNotification : ObservableObject
{
    public SelectableNotification(NotificationModel model)
    {
        Model = model;
    }

    public NotificationModel Model { get; }

    [ObservableProperty] private bool _isSelected;
}
