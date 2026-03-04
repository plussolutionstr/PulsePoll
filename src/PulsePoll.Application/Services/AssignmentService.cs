using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class AssignmentService(
    IProjectRepository repository,
    IReferralRewardService referralRewardService,
    IMessagePublisher publisher) : IAssignmentService
{
    public async Task<string> StartAsync(int projectId, int subjectId)
    {
        var project = await repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        if (project.Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
            throw new BusinessException("PROJECT_CLOSED", "Proje kapalı olduğu için başlatılamaz.");

        var assignment = await repository.GetAssignmentAsync(projectId, subjectId)
            ?? throw new ForbiddenException("Bu projeye atanmış değilsiniz.");

        if (assignment.Status != AssignmentStatus.NotStarted)
            throw new BusinessException("ASSIGNMENT_ALREADY_STARTED", "Bu proje zaten başlatılmış.");

        var separator = project.SurveyUrl.Contains('?') ? "&" : "?";
        return $"{project.SurveyUrl}{separator}{project.SubjectParameterName}={subjectId}";
    }

    public async Task MarkCompletedAsync(int projectId, int subjectId, string webhookPayload)
    {
        var assignment = await repository.GetAssignmentAsync(projectId, subjectId)
            ?? throw new NotFoundException("Proje ataması");

        var project = await repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        // Webhook duplicate çağrılarında idempotent davran.
        if (assignment.Status == AssignmentStatus.Completed)
            return;

        assignment.Status               = AssignmentStatus.Completed;
        assignment.CompletedAt          = DateTime.UtcNow;
        assignment.EarnedAmount         = project.Reward;
        assignment.RewardStatus         = RewardStatus.Pending;
        assignment.RewardProcessedAt    = null;
        assignment.RewardProcessedBy    = null;
        assignment.RewardRejectionReason = null;
        await repository.UpdateAssignmentAsync(assignment);

        await referralRewardService.TryGrantAsync(
            subjectId,
            ReferralRewardTriggerType.FirstSurveyCompleted,
            actorId: 0);

        await publisher.PublishAsync(
            new SurveyCompletedMessage(projectId, subjectId, project.Reward, webhookPayload, DateTime.UtcNow),
            Queues.SurveyCompleted);
    }
}
