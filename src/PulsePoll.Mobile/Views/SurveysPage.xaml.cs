using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class SurveysPage : ContentPage
{
    public SurveysPage(SurveysViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
