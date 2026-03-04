using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
