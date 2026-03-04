using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class ActiveQuestionPage : ContentPage
{
    public ActiveQuestionPage(ActiveQuestionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
