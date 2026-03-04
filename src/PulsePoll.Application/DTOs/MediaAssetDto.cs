namespace PulsePoll.Application.DTOs;

public record MediaAssetDto(int Id, string Name, string ContentType, long Size, string Url, int UsageCount);
