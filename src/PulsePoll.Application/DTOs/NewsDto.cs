namespace PulsePoll.Application.DTOs;

public record NewsDto(
    int Id,
    string Title,
    string Summary,
    string ImageUrl,
    int? MediaAssetId,
    string? LinkUrl,
    DateTime StartsAt,
    DateTime EndsAt,
    int Order,
    bool IsActive);

public record CreateNewsDto(
    string Title,
    string Summary,
    string? LinkUrl,
    DateTime StartsAt,
    DateTime EndsAt,
    int Order,
    bool IsActive,
    int? MediaAssetId,
    Stream? ImageStream,
    string? ImageFileName);
