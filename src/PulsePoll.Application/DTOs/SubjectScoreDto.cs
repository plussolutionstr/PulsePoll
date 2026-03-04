namespace PulsePoll.Application.DTOs;

public record SubjectScoreDto(
    int SubjectId,
    decimal Score,
    int Star,
    decimal CoreScore,
    decimal ActivityMultiplier,
    DateTime CalculatedAt,
    int TotalAssignments,
    int Started,
    int Completed,
    int NotStarted,
    int Partial,
    int Disqualify,
    int ScreenOut,
    int QuotaFull,
    int RewardApproved,
    int RewardRejected,
    int? MedianCompletionMinutes,
    int? ActiveDays30,
    DateTime? LastSeenAt);

