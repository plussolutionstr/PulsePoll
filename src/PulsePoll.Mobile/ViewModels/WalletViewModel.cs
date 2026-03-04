using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class WalletViewModel : ObservableObject
{
    public WalletViewModel(MockDataService dataService)
    {
        var wallet = dataService.GetWallet();
        WithdrawableBalance = wallet.WithdrawableBalance;
        Points = wallet.Points;
        TotalEarned = wallet.TotalEarned;
        BankAccounts = new ObservableCollection<BankAccountModel>(wallet.BankAccounts);
        RecentTransactions = new ObservableCollection<TransactionModel>(wallet.RecentTransactions);
    }

    [ObservableProperty] private decimal _withdrawableBalance;
    [ObservableProperty] private int _points;
    [ObservableProperty] private decimal _totalEarned;
    [ObservableProperty] private ObservableCollection<BankAccountModel> _bankAccounts = [];
    [ObservableProperty] private ObservableCollection<TransactionModel> _recentTransactions = [];
}
