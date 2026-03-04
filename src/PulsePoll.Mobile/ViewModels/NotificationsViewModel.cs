using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly List<NotificationModel> _allNotifications;

    public NotificationsViewModel(MockDataService dataService)
    {
        _allNotifications = dataService.GetNotifications();
        LoadData();
    }

    [ObservableProperty] private ObservableCollection<NotificationModel> _notifications = [];
    [ObservableProperty] private string _selectedFilter = "Tümü";

    public List<string> Filters => ["Tümü", "Anketler", "Kazançlar", "Sistem"];

    [RelayCommand]
    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
        LoadData();
    }

    [RelayCommand]
    private void MarkAllRead()
    {
        var updated = _allNotifications.Select(n => n with { IsRead = true }).ToList();
        _allNotifications.Clear();
        _allNotifications.AddRange(updated);
        LoadData();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private void LoadData()
    {
        var items = _allNotifications.AsEnumerable();

        items = SelectedFilter switch
        {
            "Anketler" => items.Where(n => n.Type is "survey" or "disqualified"),
            "Kazançlar" => items.Where(n => n.Type is "earning" or "rank"),
            "Sistem" => items.Where(n => n.Type == "system"),
            _ => items
        };

        Notifications = new ObservableCollection<NotificationModel>(items.OrderByDescending(n => n.Date));
    }
}
