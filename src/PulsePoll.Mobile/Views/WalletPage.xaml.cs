using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class WalletPage : ContentPage
{
    public WalletPage(WalletViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
