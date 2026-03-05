using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IAssignmentService
{
    Task<string> StartAsync(int projectId, int subjectId);
    Task MarkResultAsync(int projectId, int subjectId, AssignmentStatus status, string? webhookPayload = null);
    Task MarkCompletedAsync(int projectId, int subjectId, string webhookPayload);
}
