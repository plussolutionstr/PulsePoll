using System.Text.Json;

namespace PulsePoll.Mobile;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string? ApiBaseUrl_Android { get; set; }
    public string? SentryDsn { get; set; }
    public bool SentryDebug { get; set; }
    public double SentryTracesSampleRate { get; set; } = 1.0;
    public bool SentryEnableLogs { get; set; } = true;

    public static AppSettings Load()
    {
#if DEBUG
        const string fileName = "appsettings.Development.json";
#else
        const string fileName = "appsettings.json";
#endif
        using var stream = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
        var settings = JsonSerializer.Deserialize<AppSettings>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

#if ANDROID
        if (!string.IsNullOrEmpty(settings.ApiBaseUrl_Android))
            settings.ApiBaseUrl = settings.ApiBaseUrl_Android;
#endif

        return settings;
    }
}
