namespace PulsePoll.Domain.Entities;

public class SubjectScoreConfig : EntityBase
{
    public decimal ParticipationWeight { get; set; } = 0.25m;
    public decimal CompletionWeight { get; set; } = 0.30m;
    public decimal QualityWeight { get; set; } = 0.20m;
    public decimal ApprovalTrustWeight { get; set; } = 0.15m;
    public decimal SpeedWeight { get; set; } = 0.10m;

    public int ConfidencePivot { get; set; } = 20;
    public decimal ScoreBaseline { get; set; } = 60m;

    public decimal Star1Max { get; set; } = 44m;
    public decimal Star2Max { get; set; } = 59m;
    public decimal Star3Max { get; set; } = 74m;
    public decimal Star4Max { get; set; } = 89m;

    public int VeryActiveLastSeenDays { get; set; } = 3;
    public int ActiveLastSeenDays { get; set; } = 7;
    public int WarmLastSeenDays { get; set; } = 14;
    public int CoolingLastSeenDays { get; set; } = 30;
    public int VeryActiveMinDays30 { get; set; } = 10;

    public decimal VeryActiveMultiplier { get; set; } = 1.15m;
    public decimal ActiveMultiplier { get; set; } = 1.08m;
    public decimal WarmMultiplier { get; set; } = 1.00m;
    public decimal CoolingMultiplier { get; set; } = 0.93m;
    public decimal DormantMultiplier { get; set; } = 0.85m;
    public decimal NoTelemetryMultiplier { get; set; } = 1.00m;
}

