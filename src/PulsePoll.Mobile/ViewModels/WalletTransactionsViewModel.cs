using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class WalletTransactionsViewModel : ObservableObject
{
    private const int PageSize = 20;

    private readonly IPulsePollApiClient _apiClient;
    private int _currentPage;

    public WalletTransactionsViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isLoadingMore;
    [ObservableProperty] private bool _hasMore = true;
    [ObservableProperty] private ObservableCollection<TransactionModel> _transactions = [];

    public async Task InitializeAsync()
    {
        if (Transactions.Count > 0 || IsLoading)
            return;

        await ReloadAsync();
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        try
        {
            _currentPage = 0;
            HasMore = true;
            MainThread.BeginInvokeOnMainThread(() => Transactions.Clear());
            await LoadMoreInternalAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsLoading || IsLoadingMore || !HasMore)
            return;

        await LoadMoreInternalAsync();
    }

    private async Task LoadMoreInternalAsync()
    {
        IsLoadingMore = true;
        try
        {
            var nextPage = _currentPage + 1;
            var items = await _apiClient.GetWalletTransactionsAsync(nextPage, PageSize);
            var mapped = items.Select(t => t.ToModel()).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var item in mapped)
                    Transactions.Add(item);
            });

            _currentPage = nextPage;
            HasMore = items.Count >= PageSize;
        }
        catch
        {
            HasMore = false;
        }
        finally
        {
            IsLoadingMore = false;
        }
    }
}
