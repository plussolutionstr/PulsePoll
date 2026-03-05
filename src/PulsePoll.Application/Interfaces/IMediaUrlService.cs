namespace PulsePoll.Application.Interfaces;

public interface IMediaUrlService
{
    Task<string> GetMediaUrlAsync(string bucketName, string objectKey);
}
