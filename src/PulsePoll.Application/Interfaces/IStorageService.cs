namespace PulsePoll.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(string bucketName, string objectName, Stream stream, string contentType);
    Task DeleteAsync(string bucketName, string objectName);
    Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 3600);
    Task<(Stream Stream, string ContentType)> GetObjectStreamAsync(string bucketName, string objectName);
}
