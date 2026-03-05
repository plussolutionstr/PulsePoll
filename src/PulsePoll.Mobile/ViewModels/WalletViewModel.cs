using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class WalletViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly MockDataService _mockDataService;

    public WalletViewModel(IPulsePollApiClient apiClient, MockDataService mockDataService)
    {
        _apiClient = apiClient;
        _mockDataService = mockDataService;
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private decimal _withdrawableBalance;
    [ObservableProperty] private decimal _pendingBalance;
    [ObservableProperty] private decimal _totalEarned;
    [ObservableProperty] private decimal _unitTryMultiplier = 1m;
    [ObservableProperty] private string _rewardUnitLabel = "Poll";
    [ObservableProperty] private ObservableCollection<BankAccountModel> _bankAccounts = [];
    [ObservableProperty] private ObservableCollection<TransactionModel> _recentTransactions = [];

    public string PointsDisplay => $"{WithdrawableBalance:0.##} {RewardUnitLabel}";
    public string WithdrawableBalanceDisplay => FormatTry(ToTry(WithdrawableBalance));
    public string TotalEarnedDisplay => FormatTry(ToTry(WithdrawableBalance + PendingBalance));

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        try
        {
            await LoadFromApiAsync();
        }
        catch
        {
            LoadFromMock();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> AddBankAccountAsync(int bankId, string iban)
    {
        if (IsBusy)
            return false;

        IsBusy = true;
        try
        {
            await _apiClient.AddBankAccountAsync(bankId, iban);
            await LoadCommand.ExecuteAsync(null);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> RequestWithdrawalAsync(int bankAccountId, decimal amount)
    {
        if (IsBusy)
            return false;

        IsBusy = true;
        try
        {
            await _apiClient.RequestWithdrawalAsync(amount, bankAccountId);
            await LoadCommand.ExecuteAsync(null);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToHistoryAsync()
    {
        await Shell.Current.GoToAsync("wallet-transactions");
    }

    [RelayCommand]
    private async Task NavigateToAddBankAccountAsync()
    {
        await Shell.Current.GoToAsync("wallet-add-bank");
    }

    [RelayCommand]
    private async Task NavigateToEditBankAccountAsync(BankAccountModel? account)
    {
        if (account is null)
            return;

        await Shell.Current.GoToAsync($"wallet-add-bank?accountId={account.Id}");
    }

    [RelayCommand]
    private async Task NavigateToWithdrawAsync()
    {
        await Shell.Current.GoToAsync("wallet-withdraw");
    }

    private async Task LoadFromApiAsync()
    {
        var wallet = await _apiClient.GetWalletAsync();
        if (wallet is null)
            throw new InvalidOperationException("Wallet verisi bulunamadı.");

        var banks = await SafeCallAsync(() => _apiClient.GetBankAccountsAsync(), []);
        var transactions = await SafeCallAsync(() => _apiClient.GetWalletTransactionsAsync(page: 1, pageSize: 10), []);

        var unitLabel = string.IsNullOrWhiteSpace(wallet.UnitLabel) ? "Poll" : wallet.UnitLabel;
        var recentTransactions = transactions
            .Select(t => t.ToModel())
            .ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            WithdrawableBalance = wallet.Balance;
            PendingBalance = wallet.PendingBalance;
            TotalEarned = wallet.TotalEarned;
            UnitTryMultiplier = wallet.UnitTryMultiplier > 0m ? wallet.UnitTryMultiplier : 1m;
            RewardUnitLabel = unitLabel;
            BankAccounts = new ObservableCollection<BankAccountModel>(banks.Select(b => b.ToModel()));
            RecentTransactions = new ObservableCollection<TransactionModel>(recentTransactions);
        });
    }

    private void LoadFromMock()
    {
        var wallet = _mockDataService.GetWallet();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            WithdrawableBalance = wallet.WithdrawableBalance;
            PendingBalance = Math.Max(wallet.TotalEarned - wallet.WithdrawableBalance, 0m);
            TotalEarned = wallet.TotalEarned;
            UnitTryMultiplier = 1m;
            RewardUnitLabel = wallet.RewardUnitLabel;
            BankAccounts = new ObservableCollection<BankAccountModel>(wallet.BankAccounts);
            RecentTransactions = new ObservableCollection<TransactionModel>(wallet.RecentTransactions);
        });
    }

    partial void OnWithdrawableBalanceChanged(decimal value)
    {
        OnPropertyChanged(nameof(PointsDisplay));
        OnPropertyChanged(nameof(WithdrawableBalanceDisplay));
        OnPropertyChanged(nameof(TotalEarnedDisplay));
    }

    partial void OnPendingBalanceChanged(decimal value) => OnPropertyChanged(nameof(TotalEarnedDisplay));
    partial void OnTotalEarnedChanged(decimal value) => OnPropertyChanged(nameof(TotalEarnedDisplay));
    partial void OnUnitTryMultiplierChanged(decimal value)
    {
        OnPropertyChanged(nameof(WithdrawableBalanceDisplay));
        OnPropertyChanged(nameof(TotalEarnedDisplay));
    }

    partial void OnRewardUnitLabelChanged(string value)
    {
        OnPropertyChanged(nameof(PointsDisplay));
        OnPropertyChanged(nameof(WithdrawableBalanceDisplay));
        OnPropertyChanged(nameof(TotalEarnedDisplay));
    }

    private static async Task<T> SafeCallAsync<T>(Func<Task<T>> operation, T fallback)
    {
        try
        {
            return await operation();
        }
        catch
        {
            return fallback;
        }
    }

    private decimal ToTry(decimal amountInUnit) => decimal.Round(amountInUnit * UnitTryMultiplier, 2, MidpointRounding.AwayFromZero);

    private static string FormatTry(decimal amount)
        => $"₺{amount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"))}";
}
