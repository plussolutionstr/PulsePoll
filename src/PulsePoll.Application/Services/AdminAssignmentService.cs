using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class AdminAssignmentService(
    IProjectRepository projectRepository,
    ISubjectAssignmentJobRepository jobRepository,
    IWalletRepository walletRepository,
    IReferralRewardService referralRewardService,
    IAffiliateRewardService affiliateRewardService,
    IMessagePublisher publisher,
    ILogger<AdminAssignmentService> logger) : IAdminAssignmentService
{
    public async Task<int> RequestBulkAssignAsync(int projectId, IEnumerable<int> subjectIds, int adminId)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            throw new BusinessException("NO_SUBJECTS", "En az bir denek seçiniz.");

        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        if (project.Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
            throw new BusinessException("PROJECT_CLOSED", "Kapalı projeye denek atanamaz.");

        var job = new SubjectAssignmentJob
        {
            ProjectId = projectId,
            AdminId = adminId,
            RequestedCount = ids.Length,
            Status = AssignmentJobStatus.Pending
        };

        var jobId = await jobRepository.AddAsync(job);

        await publisher.PublishAsync(
            new SubjectAssignmentRequestedMessage(jobId, projectId, ids, adminId),
            Queues.SubjectAssignmentRequested);

        logger.LogInformation(
            "Bulk assignment requested: ProjectId={ProjectId} SubjectCount={Count} AdminId={AdminId}",
            projectId, ids.Length, adminId);

        return jobId;
    }

    public async Task<List<ProjectAssignmentDto>> GetAssignmentsAsync(int projectId)
    {
        var assignments = await projectRepository.GetAssignmentsByProjectAsync(projectId);
        return assignments.Select(a => new ProjectAssignmentDto(
            a.SubjectId,
            a.Subject.FullName,
            a.Subject.PhoneNumber,
            a.Subject.City?.Name ?? string.Empty,
            a.Subject.Gender,
            a.Subject.Age,
            a.Subject.SocioeconomicStatus?.Code,
            a.Status,
            a.AssignedAt,
            a.CompletedAt,
            a.EarnedAmount ?? 0m,
            a.RewardStatus)).ToList();
    }

    public Task<List<int>> GetAssignedSubjectIdsAsync(int projectId)
        => projectRepository.GetAssignedSubjectIdsAsync(projectId);

    public async Task RemoveAssignmentAsync(int projectId, int subjectId)
    {
        var assignment = await projectRepository.GetAssignmentAsync(projectId, subjectId)
            ?? throw new NotFoundException("Proje ataması");

        if (assignment.Status is not (AssignmentStatus.NotStarted or AssignmentStatus.Scheduled))
            throw new BusinessException("ASSIGNMENT_CANNOT_REMOVE", "Sadece başlanmamış veya zamanlanmış denekler projeden çıkarılabilir.");

        await projectRepository.RemoveAssignmentAsync(projectId, subjectId);
        logger.LogInformation("Assignment removed: ProjectId={ProjectId} SubjectId={SubjectId}",
            projectId, subjectId);
    }

    public async Task<RemoveAssignmentsResultDto> RemoveAssignmentsAsync(int projectId, IEnumerable<int> subjectIds, int adminId)
    {
        _ = adminId;
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            throw new BusinessException("NO_SUBJECTS", "En az bir denek seçiniz.");

        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        var assignments = await projectRepository.GetAssignmentsByProjectAndSubjectsAsync(projectId, ids);
        var removableIds = assignments
            .Where(a => a.Status is AssignmentStatus.NotStarted or AssignmentStatus.Scheduled)
            .Select(a => a.SubjectId)
            .ToArray();

        var removed = await projectRepository.RemoveAssignmentsAsync(projectId, removableIds);
        var skipped = ids.Length - removed;

        logger.LogInformation(
            "Assignments removed in bulk: ProjectId={ProjectId} Requested={Requested} Removed={Removed} Skipped={Skipped}",
            projectId, ids.Length, removed, skipped);

        return new RemoveAssignmentsResultDto(ids.Length, removed, skipped);
    }

    public async Task<RewardProcessResultDto> ApproveRewardsAsync(int projectId, IEnumerable<int> subjectIds, int adminId)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            throw new BusinessException("NO_SUBJECTS", "En az bir denek seçiniz.");

        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        var assignments = await projectRepository.GetAssignmentsByProjectAndSubjectsAsync(projectId, ids);
        var assignmentMap = assignments.ToDictionary(a => a.SubjectId);

        var processed = 0;
        var skipped = 0;
        decimal processedAmount = 0m;

        foreach (var subjectId in ids)
        {
            if (!assignmentMap.TryGetValue(subjectId, out var assignment))
            {
                skipped++;
                continue;
            }

            if (!IsRewardProcessableStatus(assignment.Status))
            {
                skipped++;
                continue;
            }

            if (assignment.RewardStatus is RewardStatus.Approved or RewardStatus.Rejected)
            {
                skipped++;
                continue;
            }

            var earnedAmount = assignment.EarnedAmount ?? 0m;
            if (earnedAmount > 0)
            {
                var wallet = await walletRepository.GetBySubjectIdAsync(assignment.SubjectId);
                if (wallet is null)
                {
                    skipped++;
                    continue;
                }

                var referenceId = $"assignment:{assignment.ProjectId}:{assignment.SubjectId}";
                var existingTx = await walletRepository.GetTransactionByReferenceAsync(wallet.Id, referenceId);
                if (existingTx is null)
                {
                    wallet.Balance += earnedAmount;
                    wallet.TotalEarned += earnedAmount;
                    wallet.SetUpdated(adminId);
                    await walletRepository.UpdateAsync(wallet);

                    var tx = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = earnedAmount,
                        Type = WalletTransactionType.Credit,
                        ReferenceId = referenceId,
                        Description = $"Anket ödülü: {project.Name}"
                    };
                    tx.SetCreated(adminId);
                    await walletRepository.AddTransactionAsync(tx);
                }

                await referralRewardService.TryGrantAsync(
                    subjectId,
                    ReferralRewardTriggerType.FirstRewardApproved,
                    adminId);

                await affiliateRewardService.TryGrantAsync(
                    subjectId,
                    ReferralRewardTriggerType.FirstRewardApproved,
                    adminId);
                processedAmount += earnedAmount;
            }

            assignment.RewardStatus = RewardStatus.Approved;
            assignment.RewardProcessedAt = TurkeyTime.Now;
            assignment.RewardProcessedBy = adminId;
            assignment.RewardRejectionReason = null;
            assignment.SetUpdated(adminId);
            await projectRepository.UpdateAssignmentAsync(assignment);
            processed++;
        }

        logger.LogInformation(
            "Rewards approved in bulk: ProjectId={ProjectId} Requested={Requested} Processed={Processed} Skipped={Skipped} Amount={Amount}",
            projectId, ids.Length, processed, skipped, processedAmount);

        return new RewardProcessResultDto(ids.Length, processed, skipped, processedAmount);
    }

    public async Task<RewardProcessResultDto> RejectRewardsAsync(int projectId, IEnumerable<int> subjectIds, string reason, int adminId)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            throw new BusinessException("NO_SUBJECTS", "En az bir denek seçiniz.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException("REASON_REQUIRED", "Red nedeni zorunludur.");

        _ = await projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        var assignments = await projectRepository.GetAssignmentsByProjectAndSubjectsAsync(projectId, ids);
        var assignmentMap = assignments.ToDictionary(a => a.SubjectId);

        var processed = 0;
        var skipped = 0;
        decimal processedAmount = 0m;

        foreach (var subjectId in ids)
        {
            if (!assignmentMap.TryGetValue(subjectId, out var assignment))
            {
                skipped++;
                continue;
            }

            if (!IsRewardProcessableStatus(assignment.Status))
            {
                skipped++;
                continue;
            }

            if (assignment.RewardStatus is RewardStatus.Approved or RewardStatus.Rejected)
            {
                skipped++;
                continue;
            }

            processedAmount += assignment.EarnedAmount ?? 0m;
            assignment.RewardStatus = RewardStatus.Rejected;
            assignment.RewardProcessedAt = TurkeyTime.Now;
            assignment.RewardProcessedBy = adminId;
            assignment.RewardRejectionReason = reason.Trim();
            assignment.SetUpdated(adminId);
            await projectRepository.UpdateAssignmentAsync(assignment);
            processed++;
        }

        logger.LogInformation(
            "Rewards rejected in bulk: ProjectId={ProjectId} Requested={Requested} Processed={Processed} Skipped={Skipped} Amount={Amount}",
            projectId, ids.Length, processed, skipped, processedAmount);

        return new RewardProcessResultDto(ids.Length, processed, skipped, processedAmount);
    }

    public async Task<List<SubjectAssignmentJobDto>> GetJobsAsync(int projectId)
    {
        var jobs = await jobRepository.GetByProjectIdAsync(projectId);
        return jobs.Select(j => new SubjectAssignmentJobDto(
            j.Id, j.RequestedCount, j.AssignedCount, j.SkippedCount,
            j.Status, j.CreatedAt, j.CompletedAt)).ToList();
    }

    public async Task CancelJobAsync(int jobId)
    {
        var job = await jobRepository.GetByIdAsync(jobId)
            ?? throw new NotFoundException("Atama işi");

        if (job.Status is not (AssignmentJobStatus.Pending or AssignmentJobStatus.Processing))
            throw new BusinessException("JOB_NOT_CANCELLABLE", "Yalnızca bekleyen veya işlenen işler iptal edilebilir.");

        job.Status = AssignmentJobStatus.Failed;
        job.CompletedAt = TurkeyTime.Now;
        await jobRepository.UpdateAsync(job);

        logger.LogInformation("Atama işi iptal edildi: JobId={JobId}", jobId);
    }

    private static bool IsRewardProcessableStatus(AssignmentStatus status)
        => status is AssignmentStatus.Completed
            or AssignmentStatus.Disqualify
            or AssignmentStatus.QuotaFull
            or AssignmentStatus.ScreenOut;
}
