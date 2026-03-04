using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly MockDataService _dataService;
    private List<HistoryGroup> _allGroups = [];

    public HistoryViewModel(MockDataService dataService)
    {
        _dataService = dataService;
        _allGroups = _dataService.GetHistory();
        LoadData();
        CalculateSummary();
    }

    [ObservableProperty] private ObservableCollection<HistoryGroup> _groups = [];
    [ObservableProperty] private string _selectedFilter = "Tümü";
    [ObservableProperty] private int _completedCount;
    [ObservableProperty] private int _disqualifiedCount;
    [ObservableProperty] private decimal _totalEarned;
    [ObservableProperty] private ObservableCollection<HistoryItemModel> _flatItems = [];

    public List<string> Filters => ["Tümü", "Tamamlandı", "Elendi", "Devam Ediyor"];

    [RelayCommand]
    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
        LoadData();
    }

    private void LoadData()
    {
        var items = _allGroups.SelectMany(g => g.Items);

        if (SelectedFilter != "Tümü")
            items = items.Where(i => i.Status == SelectedFilter);

        FlatItems = new ObservableCollection<HistoryItemModel>(items.OrderByDescending(i => i.Date));
    }

    private void CalculateSummary()
    {
        var allItems = _allGroups.SelectMany(g => g.Items).ToList();
        CompletedCount = allItems.Count(i => i.Status == "Tamamlandı");
        DisqualifiedCount = allItems.Count(i => i.Status == "Elendi");
        TotalEarned = allItems.Where(i => i.Reward.HasValue).Sum(i => i.Reward!.Value);
    }
}
