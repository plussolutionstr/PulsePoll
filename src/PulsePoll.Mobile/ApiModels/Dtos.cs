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
    bool IsActive,
    bool IsSeen);

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

public record ProjectHistoryApiDto(
    int ProjectId,
    string ProjectName,
    string CustomerShortName,
    AssignmentStatus Status,
    RewardStatus RewardStatus,
    DateTime AssignedAt,
    DateTime? CompletedAt,
    int DurationMinutes,
    decimal EarnedAmount,
    decimal RewardAmount,
    decimal ConsolationRewardAmount,
    string RewardUnitLabel);

public record ProjectApiDto(
    int Id,
    int CustomerId,
    string CustomerShortName,
    string Code,
    string Name,
    string? Description,
    string? Category,
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
    int? SurveyResultScriptId,
    string? SurveyResultScriptName,
    List<SurveyResultPatternApiDto>? SurveyResultPatterns,
    int? CoverMediaId,
    string? CoverImageUrl,
    string RewardUnitCode = "TRY",
    string RewardUnitLabel = "TL",
    decimal RewardUnitTryMultiplier = 1m);

public record SurveyResultPatternApiDto(
    int Id,
    AssignmentStatus Status,
    string MatchPattern,
    int Order);

public record StartProjectApiDto(string Url);

public record WalletApiDto(
    int SubjectId,
    decimal Balance,
    decimal TotalEarned,
    decimal PendingBalance,
    decimal RejectedBalance,
    DateTime UpdatedAt,
    string UnitCode,
    string UnitLabel,
    decimal UnitTryMultiplier);

public record WalletTransactionApiDto(
    int Id,
    decimal Amount,
    WalletTransactionType Type,
    string? Description,
    DateTime CreatedAt,
    bool IsManual,
    string UnitLabel);

public record BankAccountApiDto(
    int Id,
    string BankName,
    string IbanLast4,
    bool IsDefault,
    string? ThumbnailImageUrl,
    string? LogoImageUrl);

public record BankOptionApiDto(
    int Id,
    string Name,
    string? Code,
    string? ThumbnailImageUrl,
    string? LogoImageUrl);

public record AddBankAccountApiRequest(int BankId, string Iban);
public record UpdateBankAccountApiRequest(int BankId, string Iban);

public record WithdrawalRequestApiRequest(decimal Amount, int BankAccountId);

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RewardStatus
{
    None = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WalletTransactionType
{
    Credit = 0,
    Withdrawal = 1
}
