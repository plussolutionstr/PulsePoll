using System.Text.Json;

namespace PulsePoll.Mobile;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string? ApiBaseUrl_Android { get; set; }

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
