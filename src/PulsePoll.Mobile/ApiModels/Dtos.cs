using System.Text.Json.Serialization;

namespace PulsePoll.Mobile.ApiModels;

public record StoryApiDto(
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

public record NewsApiDto(
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

public record ProjectApiDto(
    int Id,
    int CustomerId,
    string CustomerShortName,
    string Code,
    string Name,
    string? Description,
    int ParticipantCount,
    int TotalTargetCount,
    int DurationDays,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal Budget,
    decimal Reward,
    decimal ConsolationReward,
    string SurveyUrl,
    string SubjectParameterName,
    int EstimatedMinutes,
    string? CustomerBriefing,
    string StartMessage,
    string CompletedMessage,
    string DisqualifyMessage,
    string QuotaFullMessage,
    string ScreenOutMessage,
    ProjectStatus Status,
    AssignmentStatus? AssignmentStatus,
    int? CoverMediaId,
    string? CoverImageUrl,
    string RewardUnitCode = "TRY",
    string RewardUnitLabel = "TL",
    decimal RewardUnitTryMultiplier = 1m);

public record AuthResultDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectStatus
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Completed = 3,
    Cancelled = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssignmentStatus
{
    NotStarted = 0,
    Completed = 1,
    Partial = 2,
    Disqualify = 3,
    QuotaFull = 4,
    ScreenOut = 5
}
