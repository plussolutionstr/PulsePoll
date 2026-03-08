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
    private List<BankOptionModel> _allBanks = [];

    public WalletAddBankAccountViewModel(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private BankOptionModel? _detectedBank;
    [ObservableProperty] private string _iban = string.Empty;
    [ObservableProperty] private string? _bankError;

    public bool HasDetectedBank => DetectedBank is not null;

    partial void OnIbanChanged(string value)
    {
        DetectBank(value);
    }

    partial void OnDetectedBankChanged(BankOptionModel? value)
    {
        OnPropertyChanged(nameof(HasDetectedBank));
    }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        try
        {
            var banks = await _apiClient.GetAvailableBanksAsync();
            _allBanks = banks
                .Select(b => b.ToModel())
                .ToList();
        }
        catch
        {
            _allBanks = [];
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

        var normalizedIban = NormalizeIban(Iban);
        if (normalizedIban.Length != 26 || !normalizedIban.StartsWith("TR", StringComparison.Ordinal) || !normalizedIban.Skip(2).All(char.IsDigit))
            return (false, "Ge\u00e7erli bir TR IBAN girin.");

        DetectBank(Iban);

        if (DetectedBank is null)
            return (false, BankError ?? "IBAN'a ait banka bulunamad\u0131. L\u00fctfen deste\u011fe ba\u015fvurun.");

        IsSaving = true;
        try
        {
            await _apiClient.AddBankAccountAsync(DetectedBank.Id, normalizedIban);
            return (true, null);
        }
        catch (HttpRequestException ex) when (!string.IsNullOrWhiteSpace(ex.Message))
        {
            return (false, ex.Message);
        }
        catch
        {
            return (false, "Banka hesab\u0131 eklenemedi.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void DetectBank(string? raw)
    {
        var iban = NormalizeIban(raw);

        if (iban.Length < 9 || !iban.StartsWith("TR", StringComparison.Ordinal))
        {
            DetectedBank = null;
            BankError = null;
            return;
        }

        var bankCode = iban.Substring(4, 5);
        var found = _allBanks.FirstOrDefault(b =>
            !string.IsNullOrEmpty(b.BankCode) &&
            b.BankCode == bankCode);

        DetectedBank = found;
        BankError = found is null
            ? "Ge\u00e7ersiz IBAN. L\u00fctfen deste\u011fe ba\u015fvurun."
            : null;
    }

    private static string NormalizeIban(string? raw)
        => (raw ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", string.Empty);
}
