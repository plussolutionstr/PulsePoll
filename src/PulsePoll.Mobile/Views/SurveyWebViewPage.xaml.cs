using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class SurveyWebViewPage : ContentPage
{
    private readonly SurveyWebViewViewModel _viewModel;
    private CancellationTokenSource? _probeCts;

    public SurveyWebViewPage(SurveyWebViewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        _probeCts?.Cancel();
        _probeCts?.Dispose();
        _probeCts = null;
        _viewModel.IsLoading = true;
    }

    private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        _viewModel.IsLoading = false;

        if (sender is not WebView webView || e.Result != WebNavigationResult.Success)
            return;

        _probeCts?.Cancel();
        _probeCts?.Dispose();
        _probeCts = new CancellationTokenSource();

        await ProbeForSurveyResultAsync(webView, _probeCts.Token);
    }

    private async Task ProbeForSurveyResultAsync(WebView webView, CancellationToken token)
    {
        const int maxAttempt = 5;
        const int delayMs = 500;

        for (var i = 0; i < maxAttempt; i++)
        {
            if (token.IsCancellationRequested)
                return;

            try
            {
                var html = await webView.EvaluateJavaScriptAsync("document.documentElement.outerHTML || ''");
                if (!string.IsNullOrWhiteSpace(html) &&
                    await _viewModel.TryHandleSurveyResultFromHtmlAsync(html))
                {
                    return;
                }
            }
            catch
            {
                // Some providers can block script evaluation for cross-domain pages.
            }

            if (i < maxAttempt - 1)
            {
                try
                {
                    await Task.Delay(delayMs, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }
    }

    protected override void OnDisappearing()
    {
        _probeCts?.Cancel();
        _probeCts?.Dispose();
        _probeCts = null;
        base.OnDisappearing();
    }
}
