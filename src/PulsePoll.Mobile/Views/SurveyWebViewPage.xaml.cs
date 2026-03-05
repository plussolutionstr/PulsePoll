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

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        _viewModel.IsLoading = true;

        if (await _viewModel.TryHandleSurveyResultUrlAsync(e.Url))
        {
            e.Cancel = true;
        }
    }

    private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        _viewModel.IsLoading = false;
        _viewModel.CurrentUrl = e.Url ?? string.Empty;

        // Bazı platformlar redirect yerine doğrudan survey-result sayfasını yükleyebilir.
        await _viewModel.TryHandleSurveyResultUrlAsync(e.Url);
    }
}
