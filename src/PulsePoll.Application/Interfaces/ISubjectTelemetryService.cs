using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectTelemetryService
{
    Task TrackActivityAsync(int subjectId, TrackSubjectActivityDto dto);
}

