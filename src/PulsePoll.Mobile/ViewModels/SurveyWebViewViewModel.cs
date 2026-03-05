using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PulsePoll.Mobile.ViewModels;

[QueryProperty(nameof(ProjectId), "projectId")]
[QueryProperty(nameof(Title), "title")]
[QueryProperty(nameof(Url), "url")]
public partial class SurveyWebViewViewModel : ObservableObject
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly MockDataService _mockDataService;
    private bool _resultHandled;
    private bool _isHandlingResult;
    private bool _patternsLoaded;
    private Task? _patternsLoadTask;
    private List<SurveyResultPatternModel> _patterns = [];
    private static readonly Regex InputTagRegex = new("<input\\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex AttributeRegex = new("(?<name>[a-zA-Z_:][\\w:.-]*)\\s*=\\s*(\"(?<dq>[^\"]*)\"|'(?<sq>[^']*)'|(?<bare>[^\\s>]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SurveyWebViewViewModel(IPulsePollApiClient apiClient, MockDataService mockDataService)
    {
        _apiClient = apiClient;
        _mockDataService = mockDataService;
    }

    [ObservableProperty] private int _projectId;
    [ObservableProperty] private string _title = "Anket";
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private bool _isLoading = true;

    public string ResolvedUrl => Uri.UnescapeDataString(Url ?? string.Empty);

    partial void OnProjectIdChanged(int value)
    {
        _patternsLoaded = false;
        _patternsLoadTask = LoadPatternsAsync(value);
    }

    partial void OnUrlChanged(string value)
    {
        OnPropertyChanged(nameof(ResolvedUrl));
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.GoToAsync("..");
    }

    public async Task<bool> TryHandleSurveyResultFromHtmlAsync(string html)
    {
        if (_resultHandled || _isHandlingResult || string.IsNullOrWhiteSpace(html))
            return false;

        await EnsurePatternsLoadedAsync();

        var preparedHtml = PrepareHtml(html);
        var matched = MatchPattern(preparedHtml);
        if (matched is null)
            return false;

        var statusText = StatusLabel(matched.Status);
        _isHandlingResult = true;
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlertAsync("Anket Sonucu", $"Durum: {statusText}", "Tamam");
                await Shell.Current.GoToAsync("..");
            });

            _resultHandled = true;
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _isHandlingResult = false;
        }
    }

    private async Task LoadPatternsAsync(int projectId)
    {
        if (projectId <= 0)
        {
            _patterns = [];
            _patternsLoaded = true;
            return;
        }

        try
        {
            var survey = await FetchOrFallbackAsync(
                () => _apiClient.GetProjectByIdAsync(projectId),
                () => _mockDataService.GetSurveyDetail(projectId));

            _patterns = survey?.ResultPatterns?
                .Where(p => !string.IsNullOrWhiteSpace(p.MatchPattern))
                .OrderBy(p => p.Order)
                .ToList() ?? [];
        }
        catch
        {
            _patterns = [];
        }
        finally
        {
            _patternsLoaded = true;
        }
    }

    private SurveyResultPatternModel? MatchPattern(string html)
    {
        var normalizedHtml = Normalize(html);
        var normalizedInputPairs = ExtractNormalizedInputPairs(html);

        foreach (var pattern in _patterns)
        {
            var raw = pattern.MatchPattern.Trim();
            if (raw.Length == 0)
                continue;

            if (html.Contains(raw, StringComparison.OrdinalIgnoreCase))
                return pattern;

            var normalizedPattern = Normalize(raw);
            if (normalizedPattern.Length == 0)
                continue;

            if (normalizedHtml.Contains(normalizedPattern, StringComparison.OrdinalIgnoreCase))
                return pattern;

            var normalizedPairPattern = NormalizePairPattern(raw);
            if (!string.IsNullOrWhiteSpace(normalizedPairPattern) &&
                normalizedInputPairs.Contains(normalizedPairPattern))
            {
                return pattern;
            }
        }

        var fallback = MatchSawtoothFallback(normalizedHtml, normalizedInputPairs);
        if (fallback is not null)
            return fallback;

        return null;
    }

    private async Task EnsurePatternsLoadedAsync()
    {
        if (_patternsLoaded)
            return;

        if (_patternsLoadTask is null && ProjectId > 0)
            _patternsLoadTask = LoadPatternsAsync(ProjectId);

        if (_patternsLoadTask is not null)
        {
            try
            {
                await _patternsLoadTask;
            }
            catch
            {
                _patterns = [];
                _patternsLoaded = true;
            }
        }
    }

    private static HashSet<string> ExtractNormalizedInputPairs(string html)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match inputMatch in InputTagRegex.Matches(html))
        {
            string? name = null;
            string? value = null;

            foreach (Match attributeMatch in AttributeRegex.Matches(inputMatch.Value))
            {
                if (!attributeMatch.Groups["name"].Success)
                    continue;

                var attributeName = attributeMatch.Groups["name"].Value.Trim();
                var attributeValue =
                    attributeMatch.Groups["dq"].Success ? attributeMatch.Groups["dq"].Value :
                    attributeMatch.Groups["sq"].Success ? attributeMatch.Groups["sq"].Value :
                    attributeMatch.Groups["bare"].Success ? attributeMatch.Groups["bare"].Value :
                    string.Empty;

                if (attributeName.Equals("name", StringComparison.OrdinalIgnoreCase))
                    name = attributeValue;
                else if (attributeName.Equals("value", StringComparison.OrdinalIgnoreCase))
                    value = attributeValue;
            }

            if (string.IsNullOrWhiteSpace(name) || value is null)
                continue;

            var normalizedPair = $"{Normalize(name)}={Normalize(value)}";
            result.Add(normalizedPair);
        }

        return result;
    }

    private static string? NormalizePairPattern(string pattern)
    {
        var index = pattern.IndexOf('=');
        if (index <= 0 || index >= pattern.Length - 1)
            return null;

        var left = pattern[..index].Trim().Trim('"', '\'');
        var right = pattern[(index + 1)..].Trim().Trim('"', '\'');
        if (left.Length == 0 || right.Length == 0)
            return null;

        return $"{Normalize(left)}={Normalize(right)}";
    }

    private static SurveyResultPatternModel? MatchSawtoothFallback(
        string normalizedHtml,
        HashSet<string> normalizedInputPairs)
    {
        if (normalizedInputPairs.Contains("hid_q_completed=completed") ||
            normalizedInputPairs.Contains("hid_destination=completed") ||
            normalizedHtml.Contains("id=completed_div", StringComparison.OrdinalIgnoreCase))
        {
            return new SurveyResultPatternModel("Completed", "__sawtooth_fallback_completed__", int.MaxValue);
        }

        if (normalizedInputPairs.Contains("hid_q_disqualified=disqualified") ||
            normalizedInputPairs.Contains("hid_destination=disqualified") ||
            normalizedHtml.Contains("id=disqualified_div", StringComparison.OrdinalIgnoreCase))
        {
            return new SurveyResultPatternModel("Disqualify", "__sawtooth_fallback_disqualify__", int.MaxValue);
        }

        if (normalizedInputPairs.Contains("hid_q_quotafull=quotafull") ||
            normalizedInputPairs.Contains("hid_destination=quotafull") ||
            normalizedHtml.Contains("id=quotafull_div", StringComparison.OrdinalIgnoreCase))
        {
            return new SurveyResultPatternModel("QuotaFull", "__sawtooth_fallback_quotafull__", int.MaxValue);
        }

        if (normalizedInputPairs.Contains("hid_q_screenout=screenout") ||
            normalizedInputPairs.Contains("hid_destination=screenout") ||
            normalizedHtml.Contains("id=screenout_div", StringComparison.OrdinalIgnoreCase))
        {
            return new SurveyResultPatternModel("ScreenOut", "__sawtooth_fallback_screenout__", int.MaxValue);
        }

        if (normalizedInputPairs.Contains("hid_q_partial=partial") ||
            normalizedInputPairs.Contains("hid_destination=partial") ||
            normalizedHtml.Contains("id=partial_div", StringComparison.OrdinalIgnoreCase))
        {
            return new SurveyResultPatternModel("Partial", "__sawtooth_fallback_partial__", int.MaxValue);
        }

        return null;
    }

    private static string Normalize(string text)
    {
        var buffer = new char[text.Length];
        var count = 0;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = char.ToLowerInvariant(text[i]);
            if (char.IsWhiteSpace(ch) || ch == '"' || ch == '\'' || ch == '\\')
                continue;

            buffer[count++] = ch;
        }

        return new string(buffer, 0, count);
    }

    private static string PrepareHtml(string rawHtml)
    {
        if (string.IsNullOrWhiteSpace(rawHtml))
            return string.Empty;

        var value = rawHtml.Trim();

        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            try
            {
                value = JsonSerializer.Deserialize<string>(value) ?? value;
            }
            catch
            {
                // Fall through to manual unescape.
            }
        }

        return value
            .Replace("\\\"", "\"", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal)
            .Replace("\\r", "\r", StringComparison.Ordinal)
            .Replace("\\t", "\t", StringComparison.Ordinal);
    }

    private static string StatusLabel(string status) => status.ToLowerInvariant() switch
    {
        "completed" => "Tamamlandı",
        "disqualify" => "Diskalifiye",
        "quotafull" => "Kota Dolu",
        "screenout" => "Elenmiş",
        "partial" => "Kısmi",
        _ => status
    };

    private static async Task<SurveyModel?> FetchOrFallbackAsync(
        Func<Task<SurveyModel?>> fetch,
        Func<SurveyModel> fallback)
    {
        try
        {
            return await fetch() ?? fallback();
        }
        catch
        {
            return fallback();
        }
    }
}
