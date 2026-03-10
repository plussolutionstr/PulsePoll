namespace PulsePoll.Domain.Entities;

public class SubjectAppActivity : EntityBase
{
    public int SubjectId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public DateTime FirstOpenAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public int OpenCount { get; set; }
    public int TotalMinutes { get; set; }
    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
    public string? DeviceIdHash { get; set; }

    public Subject Subject { get; set; } = null!;
}
