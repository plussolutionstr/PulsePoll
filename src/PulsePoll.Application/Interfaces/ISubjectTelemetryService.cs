namespace PulsePoll.Application.Interfaces;

public interface ISubjectTelemetryService
{
    Task ProcessActivityAsync(
        int subjectId,
        int activityType,
        string? platform,
        string? appVersion,
        string? deviceId,
        DateTime occurredAt,
        CancellationToken ct);
}
