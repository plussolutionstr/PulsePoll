using PulsePoll.Application.Interfaces;

namespace PulsePoll.Admin.Services;

public class ProxyMediaUrlService : IMediaUrlService
{
    public Task<string> GetMediaUrlAsync(string bucketName, string objectKey)
        => Task.FromResult($"/api/media/{bucketName}/{objectKey}");
}
