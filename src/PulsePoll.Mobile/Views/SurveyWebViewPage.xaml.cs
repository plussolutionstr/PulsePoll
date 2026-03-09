using PulsePoll.Mobile.Services;
using PulsePoll.Mobile.ViewModels;

namespace PulsePoll.Mobile.Views;

public partial class SurveyWebViewPage : ContentPage
{
    private static readonly TimeSpan AutoHelpDelay = TimeSpan.FromMilliseconds(250);
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

        // Bazı platformlar redirect yerine doğrudan survey-result sayfasını yükleyebilir.
        if (await _viewModel.TryHandleSurveyResultUrlAsync(e.Url))
            return;

        await TryAutoShowHelpAsync(e.Result);
    }

    private async void OnHelpButtonClicked(object? sender, EventArgs e)
    {
        if (_viewModel.IsHelpBusy)
            return;

        try
        {
            var result = await RunHelpLookupAsync(isAuto: false);

            if (result.Found)
            {
                await DisplayAlertAsync("Yardım", result.Message, "Tamam");
                return;
            }

            var message = string.IsNullOrWhiteSpace(_viewModel.CurrentQuestionText)
                ? "Soru algılanamadı. Sayfanın yüklenmesini bekleyin."
                : result.Message;

            await DisplayAlertAsync("Yardım", message, "Tamam");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyHelper] {ex}");
            await DisplayAlertAsync("Hata", "Yardım açılamadı. Lütfen tekrar deneyin.", "Tamam");
        }
        finally
        {
            _viewModel.IsHelpBusy = false;
        }
    }

    private async Task TryAutoShowHelpAsync(WebNavigationResult navigationResult)
    {
        if (!_viewModel.IsHelperEnabled || navigationResult != WebNavigationResult.Success || _viewModel.IsHelpBusy)
            return;

        try
        {
            await Task.Delay(AutoHelpDelay);

            var result = await RunHelpLookupAsync(isAuto: true);
            if (result.Found)
            {
                await DisplayAlertAsync("Yardım", result.Message, "Tamam");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SurveyHelper][Auto] {ex}");
        }
        finally
        {
            _viewModel.IsHelpBusy = false;
        }
    }

    private async Task<SurveyHelpMatchResult> RunHelpLookupAsync(bool isAuto)
    {
        _viewModel.IsHelpBusy = true;

        var pageText = await SurveyPageTextExtractor.ExtractAsync(SurveyWebView);
        if (isAuto && !_viewModel.ShouldRunAutoHelpForPage(pageText))
            return new SurveyHelpMatchResult(false, string.Empty);

        return await _viewModel.GetHelpAsync(pageText);
    }
}
