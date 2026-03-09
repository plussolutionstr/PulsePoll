using PulsePoll.Mobile.Services;
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

        // Bazı platformlar redirect yerine doğrudan survey-result sayfasını yükleyebilir.
        await _viewModel.TryHandleSurveyResultUrlAsync(e.Url);
    }

    private async void OnHelpButtonClicked(object? sender, EventArgs e)
    {
        if (_viewModel.IsHelpBusy)
            return;

        try
        {
            _viewModel.IsHelpBusy = true;
            var pageText = await SurveyPageTextExtractor.ExtractAsync(SurveyWebView);
            var result = await _viewModel.GetHelpAsync(pageText);

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
}
