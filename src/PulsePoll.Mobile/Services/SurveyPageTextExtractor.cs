using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PulsePoll.Mobile.Services;

public static class SurveyPageTextExtractor
{
    private const string ExtractScript =
        "(document.body && (document.body.innerText || document.body.textContent)) || document.documentElement.outerHTML || ''";

    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    public static async Task<string> ExtractAsync(WebView webView, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(webView);

        var jsTask = webView.EvaluateJavaScriptAsync(ExtractScript);
        var completedTask = await Task.WhenAny(jsTask, Task.Delay(TimeSpan.FromSeconds(5), ct));
        if (completedTask != jsTask)
            throw new TimeoutException("Sayfa metni zamanında okunamadı.");

        var raw = await jsTask;
        return Normalize(Decode(raw));
    }

    private static string Decode(string? rawValue)
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

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Normalize(NormalizationForm.FormC);
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
        return normalized.ToLower(TrCulture);
    }
}
