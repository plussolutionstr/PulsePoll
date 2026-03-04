namespace PulsePoll.Application.DTOs;

public record SubjectScoreConfigDto(
    decimal ParticipationWeight,
    decimal CompletionWeight,
    decimal QualityWeight,
    decimal ApprovalTrustWeight,
    decimal SpeedWeight,
    int ConfidencePivot,
    decimal ScoreBaseline,
    decimal Star1Max,
    decimal Star2Max,
    decimal Star3Max,
    decimal Star4Max,
    int VeryActiveLastSeenDays,
    int ActiveLastSeenDays,
    int WarmLastSeenDays,
    int CoolingLastSeenDays,
    int VeryActiveMinDays30,
    decimal VeryActiveMultiplier,
    decimal ActiveMultiplier,
    decimal WarmMultiplier,
    decimal CoolingMultiplier,
    decimal DormantMultiplier,
    decimal NoTelemetryMultiplier);

