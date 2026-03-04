namespace PulsePoll.Application.Interfaces;

public interface IAssignmentService
{
    Task<string> StartAsync(int projectId, int subjectId);
    Task MarkCompletedAsync(int projectId, int subjectId, string webhookPayload);
}
