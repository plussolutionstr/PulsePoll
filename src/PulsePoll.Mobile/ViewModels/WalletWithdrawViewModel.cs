using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class WalletWithdrawViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    public WalletWithdrawViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _amount = string.Empty;
    [ObservableProperty] private string _rewardUnitLabel = "Poll";
    [ObservableProperty] private decimal _unitTryMultiplier = 1m;
    [ObservableProperty] private decimal _withdrawableBalanceTry;
    [ObservableProperty] private ObservableCollection<BankAccountModel> _bankAccounts = [];
    [ObservableProperty] private BankAccountModel? _selectedBankAccount;
    public string ConversionInfo => $"Tutarı TL girersiniz. 1 {RewardUnitLabel} = ₺{UnitTryMultiplier.ToString("N2", TrCulture)}";
    public string WithdrawableBalanceInfo => $"Çekilebilir bakiye: ₺{WithdrawableBalanceTry.ToString("N2", TrCulture)}";

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        try
        {
            var wallet = await _apiClient.GetWalletAsync();
            var accounts = await _apiClient.GetBankAccountsAsync();
            var mappedAccounts = accounts.Select(a => a.ToModel()).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RewardUnitLabel = string.IsNullOrWhiteSpace(wallet?.UnitLabel) ? "Poll" : wallet!.UnitLabel;
                UnitTryMultiplier = wallet?.UnitTryMultiplier > 0m ? wallet.UnitTryMultiplier : 1m;
                WithdrawableBalanceTry = decimal.Round((wallet?.Balance ?? 0m) * UnitTryMultiplier, 2, MidpointRounding.AwayFromZero);
                BankAccounts = new ObservableCollection<BankAccountModel>(mappedAccounts);
                SelectedBankAccount = mappedAccounts.FirstOrDefault();
            });
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BankAccounts = [];
                SelectedBankAccount = null;
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<(bool Success, string? Error)> SubmitAsync()
    {
        if (IsSaving)
            return (false, null);

        if (SelectedBankAccount is null)
            return (false, "Lütfen bir banka hesabı seçin.");

        if (!TryParseAmount(Amount, out var amount) || amount <= 0)
            return (false, "Lütfen geçerli bir tutar girin. Örnek: 1.000,00");

        if (amount > WithdrawableBalanceTry)
            return (false, $"{WithdrawableBalanceInfo}. Bu tutardan fazla çekemezsiniz.");

        var unitAmount = UnitTryMultiplier > 0m
            ? decimal.Round(amount / UnitTryMultiplier, 2, MidpointRounding.AwayFromZero)
            : amount;

        IsSaving = true;
        try
        {
            await _apiClient.RequestWithdrawalAsync(unitAmount, SelectedBankAccount.Id);
            return (true, null);
        }
        catch (HttpRequestException ex) when (!string.IsNullOrWhiteSpace(ex.Message))
        {
            return (false, ex.Message);
        }
        catch
        {
            return (false, "Para çekme talebi gönderilemedi.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static bool TryParseAmount(string raw, out decimal amount)
    {
        var normalized = (raw ?? string.Empty).Trim();
        if (decimal.TryParse(normalized, NumberStyles.Number, TrCulture, out amount))
            return true;

        return decimal.TryParse(normalized.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    partial void OnRewardUnitLabelChanged(string value) => OnPropertyChanged(nameof(ConversionInfo));
    partial void OnUnitTryMultiplierChanged(decimal value) => OnPropertyChanged(nameof(ConversionInfo));
    partial void OnWithdrawableBalanceTryChanged(decimal value) => OnPropertyChanged(nameof(WithdrawableBalanceInfo));
}
