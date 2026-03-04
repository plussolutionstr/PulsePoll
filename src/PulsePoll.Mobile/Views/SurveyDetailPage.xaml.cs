using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class SurveyDetailPage : ContentPage
{
    public SurveyDetailPage(SurveyDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
