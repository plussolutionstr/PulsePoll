using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class WalletAddBankAccountViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;

    public WalletAddBankAccountViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private BankOptionModel? _selectedBank;
    [ObservableProperty] private string _iban = string.Empty;
    [ObservableProperty] private ObservableCollection<BankOptionModel> _banks = [];

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        try
        {
            var banks = await _apiClient.GetAvailableBanksAsync();
            var orderedBanks = banks
                .OrderBy(b => b.Name, StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), ignoreCase: true))
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Banks = new ObservableCollection<BankOptionModel>(orderedBanks.Select(b => b.ToModel()));
                if (Banks.Count == 0)
                {
                    SelectedBank = null;
                    return;
                }

                if (SelectedBank is null)
                    SelectedBank = Banks[0];
            });
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() => Banks = []);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<(bool Success, string? Error)> SaveAsync()
    {
        if (IsSaving)
            return (false, null);

        if (SelectedBank is null)
            return (false, "Banka seçimi zorunludur.");

        var normalizedIban = NormalizeIban(Iban);
        if (normalizedIban.Length != 26 || !normalizedIban.StartsWith("TR", StringComparison.Ordinal) || !normalizedIban.Skip(2).All(char.IsDigit))
            return (false, "Geçerli bir TR IBAN girin.");

        IsSaving = true;
        try
        {
            await _apiClient.AddBankAccountAsync(SelectedBank.Id, normalizedIban);
            return (true, null);
        }
        catch (HttpRequestException ex) when (!string.IsNullOrWhiteSpace(ex.Message))
        {
            return (false, ex.Message);
        }
        catch
        {
            return (false, "Banka hesabı eklenemedi.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string NormalizeIban(string? raw)
        => (raw ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", string.Empty);
}
