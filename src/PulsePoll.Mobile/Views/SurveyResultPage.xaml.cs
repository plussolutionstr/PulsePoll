using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class SurveyResultPage : ContentPage
{
    public SurveyResultPage(SurveyResultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
