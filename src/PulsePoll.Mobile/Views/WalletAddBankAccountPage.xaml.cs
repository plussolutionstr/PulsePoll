using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class WalletAddBankAccountPage : ContentPage
{
    private readonly WalletAddBankAccountViewModel _viewModel;

    public WalletAddBankAccountPage(WalletAddBankAccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
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

        await Shell.Current.GoToAsync("..", true);
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
