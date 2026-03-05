using PulsePoll.Domain.Enums;

namespace PulsePoll.Domain.Entities;

public class SubjectAppActivity : EntityBase
{
    public int SubjectId { get; set; }
    public AppActivityType Type { get; set; } = AppActivityType.Heartbeat;
    public DateTime OccurredAt { get; set; } = TurkeyTime.Now;
    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
    public string? DeviceIdHash { get; set; }

    public Subject Subject { get; set; } = null!;
}

