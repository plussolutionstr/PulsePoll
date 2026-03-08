using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private List<HistoryGroup> _allGroups = [];
    private bool _isLoaded;

    public HistoryViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private ObservableCollection<HistoryGroup> _groups = [];
    [ObservableProperty] private string _selectedFilter = "Tümü";
    [ObservableProperty] private int _completedCount;
    [ObservableProperty] private int _disqualifiedCount;
    [ObservableProperty] private decimal _totalEarned;
    [ObservableProperty] private string _rewardUnitLabel = "Poll";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListEmpty))]
    private ObservableCollection<HistoryItemModel> _flatItems = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListEmpty))]
    private bool _isLoading = true;
    [ObservableProperty] private bool _hasConnectionError;

    public bool IsListEmpty => !IsLoading && FlatItems.Count == 0;
    public string TotalEarnedDisplay => $"{TotalEarned:0.##} {RewardUnitLabel}";

    public List<string> Filters => ["Tümü", "Tamamlandı", "Elendi", "Devam Ediyor"];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        IsLoading = true;
        try
        {
            var items = await _apiClient.GetHistoryAsync();

            _allGroups = GroupByMonth(items);
            RewardUnitLabel = items.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.RewardUnitLabel))?.RewardUnitLabel ?? "Poll";
            LoadData();
            CalculateSummary();
            _isLoaded = true;
        }
        catch
        {
            HasConnectionError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RetryConnectionAsync()
    {
        HasConnectionError = false;
        _isLoaded = false;
        await LoadAsync();
    }

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
            items = items.Where(i => ToFilterKey(i.Status) == SelectedFilter);

        FlatItems = new ObservableCollection<HistoryItemModel>(items.OrderByDescending(i => i.Date));
    }

    private void CalculateSummary()
    {
        var allItems = _allGroups.SelectMany(g => g.Items).ToList();
        CompletedCount = allItems.Count(i => i.Status == "Tamamlandı");
        DisqualifiedCount = allItems.Count(i => ToFilterKey(i.Status) == "Elendi");
        TotalEarned = allItems.Where(i => i.Reward.HasValue).Sum(i => i.Reward!.Value);
    }

    partial void OnTotalEarnedChanged(decimal value) => OnPropertyChanged(nameof(TotalEarnedDisplay));
    partial void OnRewardUnitLabelChanged(string value) => OnPropertyChanged(nameof(TotalEarnedDisplay));

    private static List<HistoryGroup> GroupByMonth(IEnumerable<HistoryItemModel> items)
    {
        return items
            .GroupBy(i => i.Date.ToString("MMMM yyyy", new CultureInfo("tr-TR")))
            .Select(g => new HistoryGroup(g.Key, g.OrderByDescending(x => x.Date).ToList()))
            .ToList();
    }

    private static string ToFilterKey(string status)
    {
        return status switch
        {
            "Tamamlandı" => "Tamamlandı",
            "Devam Ediyor" => "Devam Ediyor",
            "Diskalifiye" or "Kota Dolu" or "Elenmiş" or "Elendi" => "Elendi",
            _ => "Tümü"
        };
    }
}
