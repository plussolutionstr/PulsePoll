using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
