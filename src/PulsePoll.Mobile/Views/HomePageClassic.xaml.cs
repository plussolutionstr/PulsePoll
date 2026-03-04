using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class HomePageClassic : ContentPage
{
    public HomePageClassic(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
