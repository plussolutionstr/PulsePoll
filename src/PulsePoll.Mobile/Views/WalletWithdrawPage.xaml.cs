using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class WalletWithdrawPage : ContentPage
{
    private readonly WalletWithdrawViewModel _viewModel;

    public WalletWithdrawPage(WalletWithdrawViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync(
            "Para Çekme Talebi",
            "Para çekme talebinizi göndermek istediğinize emin misiniz?",
            "Gönder",
            "Vazgeç");

        if (!confirmed)
            return;

        var (success, error) = await _viewModel.SubmitAsync();
        if (!success)
        {
            if (!string.IsNullOrWhiteSpace(error))
                await DisplayAlertAsync("Hata", error, "Tamam");
            return;
        }

        await DisplayAlertAsync("Başarılı", "Para çekme talebiniz alındı.", "Tamam");
        await Shell.Current.GoToAsync("..", true);
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..", true);
    }
}
