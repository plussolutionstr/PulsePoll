using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class WalletAddBankAccountPage : ContentPage, IQueryAttributable
{
    private readonly WalletAddBankAccountViewModel _viewModel;

    public WalletAddBankAccountPage(WalletAddBankAccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.InitializeForCreate();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("accountId", out var raw) &&
            int.TryParse(raw?.ToString(), out var accountId) &&
            accountId > 0)
        {
            _viewModel.InitializeForEdit(accountId);
            return;
        }

        _viewModel.InitializeForCreate();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var (success, error) = await _viewModel.SaveAsync();
        if (!success)
        {
            if (!string.IsNullOrWhiteSpace(error))
                await DisplayAlertAsync("Hata", error, "Tamam");
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            "Hesabı Sil",
            "Bu banka hesabını silmek istediğinize emin misiniz?",
            "Sil",
            "Vazgeç");

        if (!confirmed)
            return;

        var (success, error) = await _viewModel.DeleteAsync();
        if (!success)
        {
            if (!string.IsNullOrWhiteSpace(error))
                await DisplayAlertAsync("Hata", error, "Tamam");
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
