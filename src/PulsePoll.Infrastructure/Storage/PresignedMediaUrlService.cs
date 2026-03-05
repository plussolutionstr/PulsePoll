using PulsePoll.Application.Interfaces;

namespace PulsePoll.Infrastructure.Storage;

public class PresignedMediaUrlService(IStorageService storageService) : IMediaUrlService
{
    private const int ExpirySeconds = 7 * 24 * 3600; // 7 gün

    public Task<string> GetMediaUrlAsync(string bucketName, string objectKey)
        => storageService.GetPresignedUrlAsync(bucketName, objectKey, ExpirySeconds);
}
