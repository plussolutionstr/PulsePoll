using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class StoryService(
    IStoryRepository repository,
    IMediaAssetRepository mediaAssetRepository,
    IStorageService storage) : IStoryService
{
    private const string MediaLibraryBucketName = "media-library";
    private const string LegacyStoryBucketName = "stories";
    private const int PresignedUrlExpirySeconds = 7 * 24 * 3600; // 7 gün

    public async Task<List<StoryDto>> GetAllAsync()
    {
        var stories = await repository.GetAllAsync();
        var dtos = new List<StoryDto>(stories.Count);

        foreach (var s in stories)
        {
            var imageUrl = await ResolveImageUrlAsync(s);
            dtos.Add(ToDto(s, imageUrl));
        }

        return dtos;
    }

    public async Task<IEnumerable<StoryDto>> GetActiveStoriesAsync()
    {
        var stories = await repository.GetActiveAsync();
        var dtos = new List<StoryDto>(stories.Count);

        foreach (var s in stories)
        {
            var imageUrl = await ResolveImageUrlAsync(s);
            dtos.Add(ToDto(s, imageUrl));
        }

        return dtos;
    }

    public async Task<StoryDto> CreateAsync(CreateStoryDto dto)
    {
        if (!dto.MediaAssetId.HasValue)
            throw new BusinessException("STORY_IMAGE_REQUIRED", "Hikaye görseli zorunludur.");

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(dto.MediaAssetId.Value)
            ?? throw new NotFoundException("Medya dosyası");

        var story = new Story
        {
            Title     = dto.Title,
            ImageUrl  = mediaAsset.ObjectKey,
            MediaAssetId = mediaAsset.Id,
            LinkUrl   = dto.LinkUrl,
            BrandName = dto.BrandName,
            StartsAt  = NormalizeToUtc(dto.StartsAt),
            EndsAt    = NormalizeToUtc(dto.EndsAt),
            Order     = dto.Order,
            IsActive  = dto.IsActive
        };
        story.SetCreated(userId: 0);

        await repository.AddAsync(story);

        var imageUrl = await ResolveImageUrlAsync(story);
        return ToDto(story, imageUrl);
    }

    public async Task UpdateAsync(int id, CreateStoryDto dto)
    {
        var story = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Hikaye");

        if (dto.MediaAssetId.HasValue && dto.MediaAssetId.Value != story.MediaAssetId)
        {
            var mediaAsset = await mediaAssetRepository.GetByIdAsync(dto.MediaAssetId.Value)
                ?? throw new NotFoundException("Medya dosyası");
            story.ImageUrl = mediaAsset.ObjectKey;
            story.MediaAssetId = mediaAsset.Id;
        }

        story.Title     = dto.Title;
        story.LinkUrl   = dto.LinkUrl;
        story.BrandName = dto.BrandName;
        story.StartsAt  = NormalizeToUtc(dto.StartsAt);
        story.EndsAt    = NormalizeToUtc(dto.EndsAt);
        story.Order     = dto.Order;
        story.IsActive  = dto.IsActive;
        story.SetUpdated(userId: 0);

        await repository.UpdateAsync(story);
    }

    public async Task ReorderAsync(IReadOnlyCollection<OrderUpdateDto> orders)
    {
        if (orders.Count == 0)
            return;

        var normalized = orders
            .Where(x => x.Id > 0 && x.Order > 0)
            .Select(x => (x.Id, x.Order))
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
            return;

        await repository.ReorderAsync(normalized);
    }

    public async Task DeleteAsync(int id)
    {
        var story = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Hikaye");

        // Legacy hikayeler stories bucket'ında durabilir. Media Library referanslı olanlar silinmez.
        if (!story.MediaAssetId.HasValue && !string.IsNullOrWhiteSpace(story.ImageUrl))
        {
            await storage.DeleteAsync(LegacyStoryBucketName, story.ImageUrl);
        }

        story.SetDeleted(userId: 0);
        await repository.DeleteAsync(story);
    }

    private static StoryDto ToDto(Story s, string imageUrl) =>
        new(s.Id, s.Title, imageUrl, s.MediaAssetId, s.LinkUrl, s.BrandName, s.StartsAt, s.EndsAt, s.Order, s.IsActive);

    private Task<string> ResolveImageUrlAsync(Story story)
    {
        if (story.MediaAsset is not null)
        {
            return storage.GetPresignedUrlAsync(MediaLibraryBucketName, story.MediaAsset.ObjectKey, PresignedUrlExpirySeconds);
        }

        var bucket = story.MediaAssetId.HasValue ? MediaLibraryBucketName : LegacyStoryBucketName;
        return storage.GetPresignedUrlAsync(bucket, story.ImageUrl, PresignedUrlExpirySeconds);
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
    };
}
