namespace PulsePoll.Application.DTOs;

public record StoryDto(
    int Id,
    string Title,
    string ImageUrl,
    int? MediaAssetId,
    string? StoryImageUrl,
    int? StoryMediaAssetId,
    string? LinkUrl,
    string? Description,
    DateTime StartsAt,
    DateTime EndsAt,
    int Order,
    bool IsActive);

public record CreateStoryDto(
    string Title,
    string? Description,
    string? LinkUrl,
    DateTime StartsAt,
    DateTime EndsAt,
    int Order,
    bool IsActive,
    int? MediaAssetId,
    int? StoryMediaAssetId,
    Stream? ImageStream,
    string? ImageFileName);
