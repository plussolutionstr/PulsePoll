using System.Globalization;
using Microsoft.Extensions.Logging;

namespace PulsePoll.Mobile.Services;

public class SurveyHelperService : ISurveyHelperService
{
    private readonly IPulsePollApiClient _apiClient;
    private readonly ILogger<SurveyHelperService> _logger;

    private static readonly (string Keyword, string Message)[] Rules =
    [
        ("hangi il", "İstanbul seçin."),
        ("ikamet", "İstanbul seçin."),
        ("yas", "35-55 arası girin."),
        ("dogum yil", "1989-2000 aralığında bir yıl seçin."),
        ("meslek", "Serbest meslek seçin.")
    ];

    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    public SurveyHelperService(IPulsePollApiClient apiClient, ILogger<SurveyHelperService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<SurveyHelpMatchResult> GetHelpAsync(int projectId, string questionText, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var normalized = Normalize(questionText);
        if (string.IsNullOrWhiteSpace(normalized))
            return new SurveyHelpMatchResult(false, "Soru algılanamadı.");

        try
        {
            var result = await _apiClient.GetSurveyHelperMatchAsync(projectId, questionText, ct);
            if (result is not null)
                return new SurveyHelpMatchResult(result.Found, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Survey helper eşleşmesi API'den alınamadı. Local fallback kullanılacak.");
        }

        foreach (var rule in Rules)
        {
            if (normalized.Contains(rule.Keyword, StringComparison.Ordinal))
                return new SurveyHelpMatchResult(true, rule.Message);
        }

        return new SurveyHelpMatchResult(false, "Bu soru için yardım bulunamadı.");
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim().ToLower(TrCulture);
    }
}
