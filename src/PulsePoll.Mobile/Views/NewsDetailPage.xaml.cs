using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class NewsDetailPage : ContentPage
{
    public NewsDetailPage(NewsDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
