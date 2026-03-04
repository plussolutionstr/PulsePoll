namespace PulsePoll.Application.DTOs;

public record StoryDto(
    int Id,
    string Title,
    string ImageUrl,
    int? MediaAssetId,
    string? LinkUrl,
    string? BrandName,
    DateTime StartsAt,
    DateTime EndsAt,
    int Order,
    bool IsActive);

public record CreateStoryDto(
    string Title,
    string? BrandName,
    string? LinkUrl,
    DateTime StartsAt,
    DateTime EndsAt,
    int Order,
    bool IsActive,
    int? MediaAssetId,
    Stream? ImageStream,
    string? ImageFileName);
