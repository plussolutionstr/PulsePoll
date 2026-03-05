using System.Text.Json;

namespace PulsePoll.Mobile;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;

    public static AppSettings Load()
    {
#if DEBUG
        const string fileName = "appsettings.Development.json";
#else
        const string fileName = "appsettings.json";
#endif
        using var stream = FileSystem.OpenAppPackageFileAsync(fileName).GetAwaiter().GetResult();
        return JsonSerializer.Deserialize<AppSettings>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}
