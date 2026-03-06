using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Services;

public class ProxyMediaUrlService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : IMediaUrlService
{
    private static readonly HashSet<string> PublicBuckets = ["media-library", "stories"];

    private readonly string? _configuredBaseUrl = string.IsNullOrWhiteSpace(configuration["Media:ProxyBaseUrl"])
        ? null
        : configuration["Media:ProxyBaseUrl"]!.TrimEnd('/');

    public Task<string> GetMediaUrlAsync(string bucketName, string objectKey)
    {
        var baseUrl = _configuredBaseUrl ?? ResolveBaseUrlFromRequest();
        var segment = PublicBuckets.Contains(bucketName) ? "public/" : "";
        return Task.FromResult($"{baseUrl}/api/media/{segment}{bucketName}/{objectKey}");
    }

    private string ResolveBaseUrlFromRequest()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return string.Empty;

        return $"{request.Scheme}://{request.Host}";
    }
}
