using Minio;
using Minio.DataModel.Args;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Infrastructure.Storage;

public class MinioStorageService(IMinioClient minio) : IStorageService
{
    public async Task<string> UploadAsync(string bucketName, string objectName, Stream stream, string contentType)
    {
        var bucketExists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!bucketExists)
            await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));

        await minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType));

        return $"{bucketName}/{objectName}";
    }

    public Task DeleteAsync(string bucketName, string objectName)
        => minio.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucketName).WithObject(objectName));

    public Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 3600)
        => minio.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expirySeconds));
}
