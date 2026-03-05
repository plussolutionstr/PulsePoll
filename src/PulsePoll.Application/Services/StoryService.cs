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
            var (previewImageUrl, storyImageUrl) = await ResolveImageUrlsAsync(s);
            dtos.Add(ToDto(s, previewImageUrl, storyImageUrl));
        }

        return dtos;
    }

    public async Task<IEnumerable<StoryDto>> GetActiveStoriesAsync(int subjectId)
    {
        var stories = await repository.GetActiveAsync();
        var seenStoryIds = await repository.GetSeenStoryIdsAsync(subjectId, stories.Select(s => s.Id).ToList());
        var orderedStories = stories
            .OrderBy(s => seenStoryIds.Contains(s.Id))
            .ThenBy(s => s.Order)
            .ToList();

        var dtos = new List<StoryDto>(stories.Count);

        foreach (var s in orderedStories)
        {
            var (previewImageUrl, storyImageUrl) = await ResolveImageUrlsAsync(s);
            dtos.Add(ToDto(s, previewImageUrl, storyImageUrl, seenStoryIds.Contains(s.Id)));
        }

        return dtos;
    }

    public async Task MarkSeenAsync(int subjectId, int storyId)
    {
        var storyExists = await repository.ExistsAsync(storyId);
        if (!storyExists)
            throw new NotFoundException("Hikaye");

        await repository.MarkSeenAsync(subjectId, storyId);
    }

    public async Task<StoryDto> CreateAsync(CreateStoryDto dto)
    {
        if (!dto.MediaAssetId.HasValue)
            throw new BusinessException("STORY_IMAGE_REQUIRED", "Hikaye görseli zorunludur.");

        var previewMediaAsset = await mediaAssetRepository.GetByIdAsync(dto.MediaAssetId.Value)
            ?? throw new NotFoundException("Medya dosyası");

        string? storyImageObjectKey = null;
        int? storyMediaAssetId = null;
        if (dto.StoryMediaAssetId.HasValue)
        {
            var storyMediaAsset = await mediaAssetRepository.GetByIdAsync(dto.StoryMediaAssetId.Value)
                ?? throw new NotFoundException("Story detay görseli");
            storyImageObjectKey = storyMediaAsset.ObjectKey;
            storyMediaAssetId = storyMediaAsset.Id;
        }

        var story = new Story
        {
            Title     = dto.Title,
            ImageUrl  = previewMediaAsset.ObjectKey,
            MediaAssetId = previewMediaAsset.Id,
            StoryImageUrl = storyImageObjectKey,
            StoryMediaAssetId = storyMediaAssetId,
            LinkUrl   = dto.LinkUrl,
            Description = dto.Description,
            StartsAt  = NormalizeToTurkeyLocal(dto.StartsAt),
            EndsAt    = NormalizeToTurkeyLocal(dto.EndsAt),
            Order     = dto.Order,
            IsActive  = dto.IsActive
        };
        story.SetCreated(userId: 0);

        await repository.AddAsync(story);

        var (previewImageUrl, storyImageUrl) = await ResolveImageUrlsAsync(story);
        return ToDto(story, previewImageUrl, storyImageUrl);
    }

    public async Task UpdateAsync(int id, CreateStoryDto dto)
    {
        var story = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Hikaye");

        if (!dto.MediaAssetId.HasValue)
            throw new BusinessException("STORY_IMAGE_REQUIRED", "Hikaye görseli zorunludur.");

        if (dto.MediaAssetId.HasValue && dto.MediaAssetId.Value != story.MediaAssetId)
        {
            var mediaAsset = await mediaAssetRepository.GetByIdAsync(dto.MediaAssetId.Value)
                ?? throw new NotFoundException("Medya dosyası");
            story.ImageUrl = mediaAsset.ObjectKey;
            story.MediaAssetId = mediaAsset.Id;
        }

        if (dto.StoryMediaAssetId != story.StoryMediaAssetId)
        {
            if (dto.StoryMediaAssetId.HasValue)
            {
                var storyMediaAsset = await mediaAssetRepository.GetByIdAsync(dto.StoryMediaAssetId.Value)
                    ?? throw new NotFoundException("Story detay görseli");
                story.StoryImageUrl = storyMediaAsset.ObjectKey;
                story.StoryMediaAssetId = storyMediaAsset.Id;
            }
            else
            {
                story.StoryImageUrl = null;
                story.StoryMediaAssetId = null;
            }
        }

        story.Title     = dto.Title;
        story.LinkUrl   = dto.LinkUrl;
        story.Description = dto.Description;
        story.StartsAt  = NormalizeToTurkeyLocal(dto.StartsAt);
        story.EndsAt    = NormalizeToTurkeyLocal(dto.EndsAt);
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

    private static StoryDto ToDto(Story s, string previewImageUrl, string storyImageUrl, bool isSeen = false) =>
        new(
            s.Id,
            s.Title,
            previewImageUrl,
            s.MediaAssetId,
            storyImageUrl,
            s.StoryMediaAssetId,
            s.LinkUrl,
            s.Description,
            s.StartsAt,
            s.EndsAt,
            s.Order,
            s.IsActive,
            isSeen);

    private async Task<(string PreviewImageUrl, string StoryImageUrl)> ResolveImageUrlsAsync(Story story)
    {
        var previewImageUrl = await ResolvePreviewImageUrlAsync(story);
        var storyImageUrl = await ResolveStoryImageUrlAsync(story, previewImageUrl);
        return (previewImageUrl, storyImageUrl);
    }

    private Task<string> ResolvePreviewImageUrlAsync(Story story)
    {
        if (story.MediaAsset is not null)
        {
            return storage.GetPresignedUrlAsync(MediaLibraryBucketName, story.MediaAsset.ObjectKey, PresignedUrlExpirySeconds);
        }

        var bucket = story.MediaAssetId.HasValue ? MediaLibraryBucketName : LegacyStoryBucketName;
        return storage.GetPresignedUrlAsync(bucket, story.ImageUrl, PresignedUrlExpirySeconds);
    }

    private async Task<string> ResolveStoryImageUrlAsync(Story story, string fallbackUrl)
    {
        if (story.StoryMediaAsset is not null)
        {
            return await storage.GetPresignedUrlAsync(MediaLibraryBucketName, story.StoryMediaAsset.ObjectKey, PresignedUrlExpirySeconds);
        }

        if (!story.StoryMediaAssetId.HasValue || string.IsNullOrWhiteSpace(story.StoryImageUrl))
            return fallbackUrl;

        return await storage.GetPresignedUrlAsync(MediaLibraryBucketName, story.StoryImageUrl, PresignedUrlExpirySeconds);
    }

    private static DateTime NormalizeToTurkeyLocal(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => TurkeyTime.FromUtc(value),
        DateTimeKind.Local => DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Unspecified)
    };
}
