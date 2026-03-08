namespace PulsePoll.Application.DTOs;

public record DistributionRunResultDto(
    int ProjectId,
    string ProjectName,
    int DistributedCount,
    int RemainingScheduled,
    int DailyQuota,
    int HourlyQuota,
    int RemainingDays,
    bool IsLastDayFlush);

public record DistributionProgressDto(
    int ProjectId,
    string ProjectName,
    int TotalAssigned,
    int ScheduledCount,
    int NotStartedCount,
    int CompletedCount,
    int OtherStatusCount,
    int DailyQuota,
    int TodayDistributed,
    int RemainingDays,
    DateOnly? StartDate,
    DateOnly? EndDate,
    TimeOnly DistributionStartHour,
    TimeOnly DistributionEndHour);

public record DistributionLogDto(
    int Id,
    DateOnly RunDate,
    TimeOnly RunTime,
    int ScheduledBefore,
    int DistributedCount,
    int DailyQuota,
    int HourlyQuota,
    int RemainingDays,
    bool IsLastDayFlush,
    DateTime CreatedAt);
