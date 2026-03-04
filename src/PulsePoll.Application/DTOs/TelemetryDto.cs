using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record TrackSubjectActivityDto(
    AppActivityType Type,
    string? Platform,
    string? AppVersion,
    string? DeviceId);

public record SubjectActivityStats(
    int SubjectId,
    int ActiveDays30,
    DateTime? LastSeenAt);
