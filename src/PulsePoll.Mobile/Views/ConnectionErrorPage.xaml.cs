using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class ConnectionErrorPage : ContentPage
{
    public ConnectionErrorPage(ConnectionErrorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
