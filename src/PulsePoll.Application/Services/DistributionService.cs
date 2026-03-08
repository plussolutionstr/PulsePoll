using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class DistributionService(
    IProjectRepository projectRepository,
    IDistributionLogRepository distributionLogRepository,
    INotificationService notificationService,
    ILogger<DistributionService> logger) : IDistributionService
{
    private static readonly TimeOnly DefaultDistributionStartHour = new(9, 0);
    private static readonly TimeOnly DefaultDistributionEndHour = new(19, 0);

    public async Task<List<DistributionRunResultDto>> RunAllHourlyDistributionsAsync()
    {
        var projects = await projectRepository.GetActiveScheduledDistributionProjectsAsync();
        var results = new List<DistributionRunResultDto>();

        foreach (var project in projects)
        {
            if (!HasValidDistributionWindow(project))
            {
                logger.LogWarning(
                    "Dağıtım turu atlandı: geçersiz pencere. ProjectId={ProjectId} Start={Start} End={End}",
                    project.Id, project.DistributionStartHour, project.DistributionEndHour);
                continue;
            }

            results.Add(await RunHourlyDistributionAsync(project));
        }

        return results;
    }

    public async Task<DistributionRunResultDto> RunHourlyDistributionAsync(int projectId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");
        EnsureScheduledProjectIsRunnable(project);
        return await RunHourlyDistributionAsync(project);
    }

    private async Task<DistributionRunResultDto> RunHourlyDistributionAsync(Project project)
    {
        var now = TurkeyTime.Now;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        // Dağıtım penceresi dışındaysa çık
        if (currentTime < project.DistributionStartHour || currentTime > project.DistributionEndHour)
        {
            return new DistributionRunResultDto(
                project.Id, project.Name,
                DistributedCount: 0,
                RemainingScheduled: await projectRepository.GetAssignmentCountByStatusAsync(project.Id, AssignmentStatus.Scheduled),
                DailyQuota: 0, HourlyQuota: 0, RemainingDays: 0, IsLastDayFlush: false);
        }

        var remainingScheduled = await projectRepository.GetAssignmentCountByStatusAsync(project.Id, AssignmentStatus.Scheduled);

        if (remainingScheduled == 0)
        {
            return new DistributionRunResultDto(
                project.Id, project.Name,
                DistributedCount: 0, RemainingScheduled: 0,
                DailyQuota: 0, HourlyQuota: 0, RemainingDays: 0, IsLastDayFlush: false);
        }

        var endDate = project.EndDate ?? today;
        var remainingDays = Math.Max(1, endDate.DayNumber - today.DayNumber + 1);
        var isLastDay = remainingDays <= 1;

        int toDistribute;
        int dailyQuota;
        int hourlyQuota;

        if (isLastDay)
        {
            // Son gün: kalan tüm Scheduled'ları gönder
            toDistribute = remainingScheduled;
            dailyQuota = remainingScheduled;
            hourlyQuota = remainingScheduled;
        }
        else
        {
            // Dinamik günlük kota
            dailyQuota = (int)Math.Ceiling((double)remainingScheduled / remainingDays);

            // Bugün zaten dağıtılan
            var todayDistributed = await distributionLogRepository.GetTodayDistributedCountAsync(project.Id, today);
            var remainingDailyTarget = Math.Max(0, dailyQuota - todayDistributed);

            if (remainingDailyTarget == 0)
            {
                return new DistributionRunResultDto(
                    project.Id, project.Name,
                    DistributedCount: 0, RemainingScheduled: remainingScheduled,
                    DailyQuota: dailyQuota, HourlyQuota: 0, RemainingDays: remainingDays, IsLastDayFlush: false);
            }

            // Kalan saatler
            var totalHours = project.DistributionEndHour.Hour - project.DistributionStartHour.Hour;
            var passedHours = currentTime.Hour - project.DistributionStartHour.Hour;
            var remainingHours = Math.Max(1, totalHours - passedHours);

            hourlyQuota = (int)Math.Ceiling((double)remainingDailyTarget / remainingHours);
            toDistribute = Math.Min(hourlyQuota, remainingScheduled);
        }

        // Scheduled assignment'lardan sıradakileri al
        var assignments = await projectRepository.GetScheduledAssignmentsAsync(project.Id, toDistribute);

        if (assignments.Count == 0)
        {
            return new DistributionRunResultDto(
                project.Id, project.Name,
                DistributedCount: 0, RemainingScheduled: remainingScheduled,
                DailyQuota: dailyQuota, HourlyQuota: hourlyQuota, RemainingDays: remainingDays, IsLastDayFlush: isLastDay);
        }

        // Scheduled → NotStarted
        var assignmentIds = assignments.Select(a => a.Id).ToList();
        await projectRepository.UpdateAssignmentsStatusBatchAsync(assignmentIds, AssignmentStatus.NotStarted, now);

        // Push notification
        var subjectIds = assignments.Select(a => a.SubjectId).ToList();
        await notificationService.SendPushToManyAsync(subjectIds, "Yeni anketin var!", project.StartMessage, "survey_assigned");

        // Log kaydı
        var log = new DistributionLog
        {
            ProjectId = project.Id,
            RunDate = today,
            RunTime = currentTime,
            ScheduledBefore = remainingScheduled,
            DistributedCount = assignments.Count,
            DailyQuota = dailyQuota,
            HourlyQuota = hourlyQuota,
            RemainingDays = remainingDays,
            IsLastDayFlush = isLastDay
        };
        log.SetCreated(0);
        await distributionLogRepository.AddAsync(log);

        logger.LogInformation(
            "Dağıtım turu: Project={ProjectId} Distributed={Count} DailyQuota={DQ} HourlyQuota={HQ} RemainingDays={RD} LastDay={LD}",
            project.Id, assignments.Count, dailyQuota, hourlyQuota, remainingDays, isLastDay);

        return new DistributionRunResultDto(
            project.Id, project.Name,
            DistributedCount: assignments.Count,
            RemainingScheduled: remainingScheduled - assignments.Count,
            DailyQuota: dailyQuota,
            HourlyQuota: hourlyQuota,
            RemainingDays: remainingDays,
            IsLastDayFlush: isLastDay);
    }

    public async Task<List<DistributionReminderResultDto>> RunDueReminderNotificationsAsync()
    {
        var now = TurkeyTime.Now;
        var currentTime = TimeOnly.FromDateTime(now);
        var projects = await projectRepository.GetActiveScheduledDistributionProjectsAsync();
        var results = new List<DistributionReminderResultDto>();

        foreach (var project in projects)
        {
            if (!HasValidDistributionWindow(project))
            {
                logger.LogWarning(
                    "Hatırlatma atlandı: geçersiz pencere. ProjectId={ProjectId} Start={Start} End={End}",
                    project.Id, project.DistributionStartHour, project.DistributionEndHour);
                continue;
            }

            if (!IsReminderDueNow(project, currentTime))
                continue;

            var count = await SendReminderNotificationsAsync(project);
            results.Add(new DistributionReminderResultDto(project.Id, project.Name, count));
        }

        return results;
    }

    public async Task<int> SendReminderNotificationsAsync(int projectId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");
        EnsureScheduledProjectIsRunnable(project);
        return await SendReminderNotificationsAsync(project);
    }

    public async Task<DistributionProgressDto> GetDistributionProgressAsync(int projectId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        var today = DateOnly.FromDateTime(TurkeyTime.Now);
        var endDate = project.EndDate ?? today;
        var remainingDays = Math.Max(0, endDate.DayNumber - today.DayNumber + 1);

        var assignmentCounts = await projectRepository.GetAssignmentStatusCountsAsync(projectId);
        var countMap = assignmentCounts.ToDictionary(x => x.Status, x => x.Count);
        var totalAssigned = assignmentCounts.Sum(x => x.Count);
        var scheduledCount = countMap.GetValueOrDefault(AssignmentStatus.Scheduled);
        var notStartedCount = countMap.GetValueOrDefault(AssignmentStatus.NotStarted);
        var completedCount = countMap.GetValueOrDefault(AssignmentStatus.Completed);
        var otherCount = totalAssigned - scheduledCount - notStartedCount - completedCount;

        var dailyQuota = remainingDays > 0 && scheduledCount > 0
            ? (int)Math.Ceiling((double)scheduledCount / remainingDays)
            : 0;

        var todayDistributed = await distributionLogRepository.GetTodayDistributedCountAsync(projectId, today);

        return new DistributionProgressDto(
            project.Id, project.Name,
            TotalAssigned: totalAssigned,
            ScheduledCount: scheduledCount,
            NotStartedCount: notStartedCount,
            CompletedCount: completedCount,
            OtherStatusCount: otherCount,
            DailyQuota: dailyQuota,
            TodayDistributed: todayDistributed,
            RemainingDays: remainingDays,
            StartDate: project.StartDate,
            EndDate: project.EndDate,
            DistributionStartHour: project.DistributionStartHour,
            DistributionEndHour: project.DistributionEndHour,
            ProjectStatus: project.Status,
            IsScheduledDistribution: project.IsScheduledDistribution,
            HasValidDistributionWindow: HasValidDistributionWindow(project));
    }

    public async Task<List<DistributionLogDto>> GetDistributionLogsAsync(int projectId)
    {
        var logs = await distributionLogRepository.GetByProjectAsync(projectId);
        return logs.Select(x => new DistributionLogDto(
            x.Id, x.RunDate, x.RunTime,
            x.ScheduledBefore, x.DistributedCount,
            x.DailyQuota, x.HourlyQuota,
            x.RemainingDays, x.IsLastDayFlush,
            x.CreatedAt)).ToList();
    }

    private async Task<int> SendReminderNotificationsAsync(Project project)
    {
        var now = TurkeyTime.Now;
        var today = DateOnly.FromDateTime(now);
        var assignments = await projectRepository.GetNotStartedNeedingReminderAsync(project.Id, today);

        if (assignments.Count == 0)
            return 0;

        var subjectIds = assignments.Select(a => a.SubjectId).ToList();
        await notificationService.SendPushToManyAsync(subjectIds, "Anketin seni bekliyor!", project.StartMessage, "survey_reminder");

        var assignmentIds = assignments.Select(a => a.Id).ToList();
        await projectRepository.UpdateAssignmentsStatusBatchAsync(assignmentIds, AssignmentStatus.NotStarted, now);

        logger.LogInformation("Hatırlatma gönderildi: Project={ProjectId} Count={Count}", project.Id, assignments.Count);
        return assignments.Count;
    }

    private static bool HasValidDistributionWindow(Project project)
        => project.DistributionStartHour >= DefaultDistributionStartHour
           && project.DistributionEndHour <= DefaultDistributionEndHour
           && project.DistributionStartHour < project.DistributionEndHour
           && project.DistributionStartHour.Minute == 0
           && project.DistributionEndHour.Minute == 0;

    private static bool IsReminderDueNow(Project project, TimeOnly currentTime)
        => currentTime == project.DistributionStartHour;

    private static void EnsureScheduledProjectIsRunnable(Project project)
    {
        if (!project.IsScheduledDistribution)
            throw new BusinessException("SCHEDULED_DISTRIBUTION_DISABLED", "Bu projede zamana yayılı dağıtım aktif değil.");

        if (project.Status != ProjectStatus.Active)
            throw new BusinessException("PROJECT_NOT_ACTIVE", "Dağıtım işlemi yalnızca aktif projelerde çalıştırılabilir.");

        if (!HasValidDistributionWindow(project))
            throw new BusinessException("INVALID_DISTRIBUTION_WINDOW", "Dağıtım saatleri 09:00-19:00 aralığında ve tam saat olacak şekilde ayarlanmalıdır.");
    }
}
