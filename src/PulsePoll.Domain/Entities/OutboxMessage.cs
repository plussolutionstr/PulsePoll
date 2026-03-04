namespace PulsePoll.Domain.Entities;

public class OutboxMessage
{
    public long Id { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? LockedUntil { get; set; }
}
