using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
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
    public async Task<List<DistributionRunResultDto>> RunAllHourlyDistributionsAsync()
    {
        var projects = await projectRepository.GetActiveScheduledDistributionProjectsAsync();
        var results = new List<DistributionRunResultDto>();

        foreach (var project in projects)
            results.Add(await RunHourlyDistributionAsync(project));

        return results;
    }

    public async Task<DistributionRunResultDto> RunHourlyDistributionAsync(int projectId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found.");
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

    public async Task<int> SendReminderNotificationsAsync(int projectId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found.");

        var today = DateOnly.FromDateTime(TurkeyTime.Now);
        var assignments = await projectRepository.GetNotStartedNeedingReminderAsync(projectId, today);

        if (assignments.Count == 0)
            return 0;

        var subjectIds = assignments.Select(a => a.SubjectId).ToList();
        await notificationService.SendPushToManyAsync(subjectIds, "Anketin seni bekliyor!", project.StartMessage, "survey_reminder");

        logger.LogInformation("Hatırlatma gönderildi: Project={ProjectId} Count={Count}", projectId, assignments.Count);
        return assignments.Count;
    }

    public async Task<DistributionProgressDto> GetDistributionProgressAsync(int projectId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new InvalidOperationException($"Project {projectId} not found.");

        var today = DateOnly.FromDateTime(TurkeyTime.Now);
        var endDate = project.EndDate ?? today;
        var remainingDays = Math.Max(0, endDate.DayNumber - today.DayNumber + 1);

        var assignments = await projectRepository.GetAssignmentsByProjectAsync(projectId);
        var totalAssigned = assignments.Count;
        var scheduledCount = assignments.Count(a => a.Status == AssignmentStatus.Scheduled);
        var notStartedCount = assignments.Count(a => a.Status == AssignmentStatus.NotStarted);
        var completedCount = assignments.Count(a => a.Status == AssignmentStatus.Completed);
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
            DistributionEndHour: project.DistributionEndHour);
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
}
