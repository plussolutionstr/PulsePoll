using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Worker.Consumers;

public class SubjectAssignmentConsumer(
    IProjectRepository projectRepository,
    ISubjectAssignmentJobRepository jobRepository,
    ILogger<SubjectAssignmentConsumer> logger) : IConsumer<SubjectAssignmentRequestedMessage>
{
    public async Task Consume(ConsumeContext<SubjectAssignmentRequestedMessage> context)
    {
        var msg = context.Message;

        var job = await jobRepository.GetByIdAsync(msg.JobId);
        if (job is null)
        {
            // Transaction commit süresi için 1.5 sn bekle ve tekrar dene
            await Task.Delay(1500);
            job = await jobRepository.GetByIdAsync(msg.JobId);
            
            if (job is null)
                throw new InvalidOperationException($"SubjectAssignmentJob veritabanında bulunamadı: {msg.JobId}. Mesaj tekrar denenecek.");
        }

        job.Status = AssignmentJobStatus.Processing;
        await jobRepository.UpdateAsync(job);

        try
        {
            var project = await projectRepository.GetByIdAsync(msg.ProjectId)
                ?? throw new InvalidOperationException($"Proje bulunamadı: {msg.ProjectId}");

            var initialStatus = project.IsScheduledDistribution
                ? AssignmentStatus.Scheduled
                : AssignmentStatus.NotStarted;

            var existingIds = await projectRepository.GetAssignedSubjectIdsAsync(msg.ProjectId);
            var existingSet = existingIds.ToHashSet();

            var now = TurkeyTime.Now;
            var toAssign = msg.SubjectIds
                .Where(id => !existingSet.Contains(id))
                .Select(id =>
                {
                    var a = new ProjectAssignment
                    {
                        ProjectId  = msg.ProjectId,
                        SubjectId  = id,
                        AssignedAt = now,
                        Status     = initialStatus
                    };
                    a.SetCreated(msg.AdminId, now);
                    return a;
                })
                .ToList();

            if (toAssign.Count > 0)
                await projectRepository.AddAssignmentsAsync(toAssign);

            job.AssignedCount = toAssign.Count;
            job.SkippedCount  = msg.SubjectIds.Length - toAssign.Count;
            job.Status        = AssignmentJobStatus.Completed;
            job.CompletedAt   = TurkeyTime.Now;
            await jobRepository.UpdateAsync(job);

            logger.LogInformation(
                "Toplu atama tamamlandı: JobId={JobId} Atanan={Assigned} Atlanan={Skipped}",
                msg.JobId, job.AssignedCount, job.SkippedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Toplu atama başarısız: JobId={JobId}", msg.JobId);
            try
            {
                job.Status = AssignmentJobStatus.Failed;
                await jobRepository.UpdateAsync(job);
            }
            catch (Exception updateEx)
            {
                logger.LogError(updateEx, "Job durumu güncellenemedi: JobId={JobId}", msg.JobId);
            }
        }
    }
}
