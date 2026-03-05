using PulsePoll.Application.Interfaces;

namespace PulsePoll.Api.Services;

public class ProxyMediaUrlService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : IMediaUrlService
{
    private readonly string? _configuredBaseUrl = string.IsNullOrWhiteSpace(configuration["Media:ProxyBaseUrl"])
        ? null
        : configuration["Media:ProxyBaseUrl"]!.TrimEnd('/');

    public Task<string> GetMediaUrlAsync(string bucketName, string objectKey)
    {
        var baseUrl = _configuredBaseUrl ?? ResolveBaseUrlFromRequest();
        return Task.FromResult($"{baseUrl}/api/media/{bucketName}/{objectKey}");
    }

    private string ResolveBaseUrlFromRequest()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return string.Empty;

        return $"{request.Scheme}://{request.Host}";
    }
}
