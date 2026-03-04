using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class MediaAssetService(
    IMediaAssetRepository repository,
    IStorageService storage) : IMediaAssetService
{
    private const string BucketName = "media-library";
    private const int PresignedUrlExpirySeconds = 7 * 24 * 3600; // 7 gün
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp"
    ];

    public async Task<List<MediaAssetDto>> GetAllAsync()
    {
        var assets = await repository.GetAllAsync();
        var dtos = new List<MediaAssetDto>(assets.Count);

        foreach (var asset in assets)
        {
            var url = await storage.GetPresignedUrlAsync(BucketName, asset.ObjectKey, PresignedUrlExpirySeconds);
            dtos.Add(ToDto(asset, url));
        }

        return dtos;
    }

    public async Task<MediaAssetDto> UploadAsync(Stream stream, string fileName, string contentType, long size, int adminId)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new BusinessException("INVALID_FILE_TYPE", "Sadece JPEG, PNG, GIF ve WebP dosyaları kabul edilir.");

        if (size > MaxFileSizeBytes)
            throw new BusinessException("FILE_TOO_LARGE", "Dosya boyutu 5 MB sınırını aşıyor.");

        var extension = Path.GetExtension(fileName);
        var objectKey = $"{Guid.NewGuid():N}{extension}";

        await storage.UploadAsync(BucketName, objectKey, stream, contentType);

        var asset = new MediaAsset
        {
            Name        = Path.GetFileNameWithoutExtension(fileName),
            ObjectKey   = objectKey,
            ContentType = contentType,
            Size        = size
        };
        asset.SetCreated(adminId);

        await repository.AddAsync(asset);

        var url = await storage.GetPresignedUrlAsync(BucketName, asset.ObjectKey, PresignedUrlExpirySeconds);
        return ToDto(asset, url);
    }

    public async Task DeleteAsync(int id, int adminId)
    {
        var asset = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Medya dosyası");

        if (asset.Projects.Any() || asset.Stories.Any() || asset.News.Any())
            throw new BusinessException("MEDIA_IN_USE", "Bu görsel kullanımda. Silmek için önce bağlı proje/story/haber kayıtlarından kaldırın.");

        await storage.DeleteAsync(BucketName, asset.ObjectKey);

        asset.SetDeleted(adminId);
        await repository.DeleteAsync(asset);
    }

    private static MediaAssetDto ToDto(MediaAsset a, string url) =>
        new(a.Id, a.Name, a.ContentType, a.Size, url, a.Projects.Count);
}
