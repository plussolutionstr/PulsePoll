using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class SurveyWebViewPage : ContentPage
{
    private readonly SurveyWebViewViewModel _viewModel;

    public SurveyWebViewPage(SurveyWebViewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        _viewModel.IsLoading = true;
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        _viewModel.IsLoading = false;
    }
}
