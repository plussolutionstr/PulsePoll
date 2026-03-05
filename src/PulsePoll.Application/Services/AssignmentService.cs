using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class AssignmentService(
    IProjectRepository repository,
    ISubjectRepository subjectRepository,
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

        if (IsTerminalStatus(assignment.Status))
            throw new BusinessException("ASSIGNMENT_ALREADY_FINISHED", "Bu proje için anket süreci tamamlanmış.");

        if (assignment.Status == AssignmentStatus.NotStarted)
        {
            assignment.Status = AssignmentStatus.Partial;
            assignment.SetUpdated(subjectId);
            await repository.UpdateAssignmentAsync(assignment);
        }

        var subject = await subjectRepository.GetByIdAsync(subjectId)
            ?? throw new NotFoundException("Denek");

        var separator = project.SurveyUrl.Contains('?') ? "&" : "?";
        return $"{project.SurveyUrl}{separator}{project.SubjectParameterName}={subject.PublicId:D}";
    }

    public async Task MarkResultAsync(int projectId, int subjectId, AssignmentStatus status, string? webhookPayload = null)
    {
        var assignment = await repository.GetAssignmentAsync(projectId, subjectId)
            ?? throw new NotFoundException("Proje ataması");

        var project = await repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        if (IsTerminalStatus(assignment.Status))
            return;

        var now = DateTime.UtcNow;
        assignment.Status = status;
        assignment.CompletedAt = IsTerminalStatus(status) ? now : null;

        var earnedAmount = ResolveEarnedAmount(project, status);
        assignment.EarnedAmount = earnedAmount > 0m ? earnedAmount : null;
        assignment.RewardStatus = earnedAmount > 0m ? RewardStatus.Pending : RewardStatus.None;
        assignment.RewardProcessedAt = null;
        assignment.RewardProcessedBy = null;
        assignment.RewardRejectionReason = null;
        assignment.SetUpdated(subjectId);
        await repository.UpdateAssignmentAsync(assignment);

        if (status != AssignmentStatus.Completed)
            return;

        await referralRewardService.TryGrantAsync(
            subjectId,
            ReferralRewardTriggerType.FirstSurveyCompleted,
            actorId: 0);

        await publisher.PublishAsync(
            new SurveyCompletedMessage(projectId, subjectId, project.Reward, webhookPayload ?? string.Empty, now),
            Queues.SurveyCompleted);
    }

    public async Task MarkCompletedAsync(int projectId, int subjectId, string webhookPayload)
    {
        await MarkResultAsync(projectId, subjectId, AssignmentStatus.Completed, webhookPayload);
    }

    private static bool IsTerminalStatus(AssignmentStatus status)
        => status is AssignmentStatus.Completed
            or AssignmentStatus.Disqualify
            or AssignmentStatus.QuotaFull
            or AssignmentStatus.ScreenOut;

    private static decimal ResolveEarnedAmount(Domain.Entities.Project project, AssignmentStatus status)
        => status switch
        {
            AssignmentStatus.Completed => project.Reward,
            AssignmentStatus.Disqualify or AssignmentStatus.QuotaFull or AssignmentStatus.ScreenOut => project.ConsolationReward,
            _ => 0m
        };
}
