using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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

    private int? _editingAccountId;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private BankOptionModel? _selectedBank;
    [ObservableProperty] private string _iban = string.Empty;
    [ObservableProperty] private ObservableCollection<BankOptionModel> _banks = [];
    public bool IsEditMode => _editingAccountId.HasValue;
    public string PageTitle => IsEditMode ? "Banka Hesabını Düzenle" : "Banka Hesabı Ekle";
    public string SaveButtonText => IsEditMode ? "Güncelle" : "Kaydet";

    public void InitializeForCreate()
    {
        _editingAccountId = null;
        Iban = string.Empty;
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    public void InitializeForEdit(int accountId)
    {
        _editingAccountId = accountId;
        Iban = string.Empty;
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

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

            BankAccountApiDto? editingAccount = null;
            if (IsEditMode)
            {
                var accounts = await _apiClient.GetBankAccountsAsync();
                editingAccount = accounts.FirstOrDefault(a => a.Id == _editingAccountId);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Banks = new ObservableCollection<BankOptionModel>(orderedBanks.Select(b => b.ToModel()));
                if (Banks.Count == 0)
                {
                    SelectedBank = null;
                    return;
                }

                if (editingAccount is not null)
                {
                    SelectedBank = Banks.FirstOrDefault(b =>
                        string.Equals(b.Name, editingAccount.BankName, StringComparison.OrdinalIgnoreCase))
                        ?? Banks[0];
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
            if (IsEditMode && _editingAccountId.HasValue)
                await _apiClient.UpdateBankAccountAsync(_editingAccountId.Value, SelectedBank.Id, normalizedIban);
            else
                await _apiClient.AddBankAccountAsync(SelectedBank.Id, normalizedIban);

            return (true, null);
        }
        catch (HttpRequestException ex) when (!string.IsNullOrWhiteSpace(ex.Message))
        {
            return (false, ex.Message);
        }
        catch
        {
            return (false, IsEditMode ? "Banka hesabı güncellenemedi." : "Banka hesabı eklenemedi.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync()
    {
        if (!IsEditMode || !_editingAccountId.HasValue)
            return (false, "Silinecek bir hesap bulunamadı.");

        if (IsSaving)
            return (false, null);

        IsSaving = true;
        try
        {
            await _apiClient.DeleteBankAccountAsync(_editingAccountId.Value);
            return (true, null);
        }
        catch (HttpRequestException ex) when (!string.IsNullOrWhiteSpace(ex.Message))
        {
            return (false, ex.Message);
        }
        catch
        {
            return (false, "Banka hesabı silinemedi.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string NormalizeIban(string? raw)
        => (raw ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", string.Empty);
}
