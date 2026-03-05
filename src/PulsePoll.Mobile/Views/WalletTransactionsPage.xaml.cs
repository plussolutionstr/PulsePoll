using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class WalletTransactionsPage : ContentPage
{
    private readonly WalletTransactionsViewModel _viewModel;

    public WalletTransactionsPage(WalletTransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
