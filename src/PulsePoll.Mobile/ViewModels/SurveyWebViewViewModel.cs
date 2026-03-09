using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(ProjectId), "projectId")]
[QueryProperty(nameof(Title), "title")]
[QueryProperty(nameof(Url), "url")]
[QueryProperty(nameof(HelperEnabledRaw), "helperEnabled")]
public partial class SurveyWebViewViewModel : ObservableObject
{
    private readonly ISurveyHelperService _surveyHelperService;
    private bool _resultHandled;

    public SurveyWebViewViewModel(ISurveyHelperService surveyHelperService)
    {
        _surveyHelperService = surveyHelperService;
    }

    [ObservableProperty] private int _projectId;
    [ObservableProperty] private string _title = "Anket";
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _helperEnabledRaw = "false";
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _isHelpBusy;
    [ObservableProperty] private string _currentQuestionText = string.Empty;

    public string ResolvedUrl => Uri.UnescapeDataString(Url ?? string.Empty);
    public bool IsHelperEnabled => bool.TryParse(HelperEnabledRaw, out var value) && value;

    partial void OnUrlChanged(string value)
    {
        OnPropertyChanged(nameof(ResolvedUrl));
    }

    partial void OnHelperEnabledRawChanged(string value)
    {
        OnPropertyChanged(nameof(IsHelperEnabled));
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    /// <summary>
    /// WebView navigating/navigated sırasında URL'yi kontrol eder.
    /// /survey-result/{status} pattern'ini yakalayıp SurveyResultPage'e yönlendirir.
    /// </summary>
    public async Task<bool> TryHandleSurveyResultUrlAsync(string? url)
    {
        if (_resultHandled || string.IsNullOrWhiteSpace(url))
            return false;

        var status = ParseStatusFromUrl(url);
        if (status is null)
            return false;

        _resultHandled = true;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var encodedStatus = Uri.EscapeDataString(status);
            await Shell.Current.GoToAsync($"surveyresult?projectId={ProjectId}&status={encodedStatus}");
        });

        return true;
    }

    /// <summary>
    /// URL'den survey-result status'unu parse eder.
    /// Desteklenen formatlar:
    ///   /survey-result/completed?sguid=...
    ///   /survey-result/set?sguid=...&amp;status=completed
    /// </summary>
    private static string? ParseStatusFromUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        // Path: /survey-result/{segment}
        var segments = uri.AbsolutePath.Trim('/').Split('/');
        if (segments.Length < 2 || !segments[0].Equals("survey-result", StringComparison.OrdinalIgnoreCase))
            return null;

        var segment = segments[1].ToLowerInvariant();

        // /survey-result/set?status=completed
        if (segment == "set")
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var statusParam = query["status"];
            return NormalizeStatus(statusParam);
        }

        // /survey-result/completed, /survey-result/disqualify, etc.
        return NormalizeStatus(segment);
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return status.Trim().ToLowerInvariant() switch
        {
            "completed" => "Completed",
            "disqualify" or "disqualified" => "Disqualify",
            "quotafull" => "QuotaFull",
            "screenout" => "ScreenOut",
            "partial" => "Partial",
            _ => null
        };
    }

    public async Task<SurveyHelpMatchResult> GetHelpAsync(string? rawQuestionText, CancellationToken ct = default)
    {
        CurrentQuestionText = DecodeJavaScriptResult(rawQuestionText);
        return await _surveyHelperService.GetHelpAsync(ProjectId, CurrentQuestionText, ct);
    }

    private static string DecodeJavaScriptResult(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return string.Empty;

        var trimmed = rawValue.Trim();
        if (trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[^1] == '"')
        {
            try
            {
                return JsonSerializer.Deserialize<string>(trimmed) ?? string.Empty;
            }
            catch
            {
                return trimmed.Trim('"');
            }
        }

        return trimmed;
    }
}
