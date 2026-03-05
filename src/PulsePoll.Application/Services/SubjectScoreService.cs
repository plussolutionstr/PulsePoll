using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class SubjectScoreService(
    ISubjectRepository subjectRepository,
    IProjectRepository projectRepository,
    ISubjectAppActivityRepository activityRepository,
    ISubjectScoreConfigRepository configRepository,
    ISubjectScoreSnapshotRepository snapshotRepository) : ISubjectScoreService
{
    public async Task<SubjectScoreDto?> GetCurrentAsync(int subjectId)
    {
        var map = await GetCurrentBulkAsync([subjectId]);
        return map.GetValueOrDefault(subjectId);
    }

    public async Task<Dictionary<int, SubjectScoreDto>> GetCurrentBulkAsync(IEnumerable<int> subjectIds)
    {
        var ids = subjectIds.Distinct().Where(x => x > 0).ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, SubjectScoreDto>();

        var assignments = await projectRepository.GetAssignmentsBySubjectIdsAsync(ids);
        var nowUtc = TurkeyTime.Now;
        var config = await LoadConfigAsync();
        var activityStats = await activityRepository.GetStatsBySubjectIdsAsync(ids, nowUtc.AddDays(-30));
        var grouped = assignments
            .GroupBy(a => a.SubjectId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var snapshots = new List<SubjectScoreSnapshot>(ids.Length);
        foreach (var subjectId in ids)
        {
            grouped.TryGetValue(subjectId, out var subjectAssignments);
            activityStats.TryGetValue(subjectId, out var stat);
            snapshots.Add(BuildSnapshot(subjectId, subjectAssignments ?? [], stat, config, nowUtc));
        }

        await snapshotRepository.UpsertManyAsync(snapshots);

        return snapshots.ToDictionary(x => x.SubjectId, MapToDto);
    }

    public async Task RecalculateAsync(int subjectId)
        => _ = await GetCurrentBulkAsync([subjectId]);

    public async Task RecalculateAllAsync()
    {
        var allIds = await subjectRepository.GetAllIdsAsync();
        _ = await GetCurrentBulkAsync(allIds);
    }

    private static SubjectScoreSnapshot BuildSnapshot(
        int subjectId,
        IReadOnlyCollection<ProjectAssignment> assignments,
        SubjectActivityStats? activityStats,
        SubjectScoreConfigDto config,
        DateTime nowUtc)
    {
        var totalAssignments = assignments.Count;
        var notStarted = assignments.Count(a => a.Status == AssignmentStatus.NotStarted);
        var started = Math.Max(0, totalAssignments - notStarted);
        var completed = assignments.Count(a => a.Status == AssignmentStatus.Completed);
        var partial = assignments.Count(a => a.Status == AssignmentStatus.Partial);
        var disqualify = assignments.Count(a => a.Status == AssignmentStatus.Disqualify);
        var screenOut = assignments.Count(a => a.Status == AssignmentStatus.ScreenOut);
        var quotaFull = assignments.Count(a => a.Status == AssignmentStatus.QuotaFull);
        var rewardApproved = assignments.Count(a => a.RewardStatus == RewardStatus.Approved);
        var rewardRejected = assignments.Count(a => a.RewardStatus == RewardStatus.Rejected);
        var medianCompletionMinutes = CalculateMedianCompletionMinutes(assignments);

        var participation = Ratio(started, totalAssignments);
        var completion = Ratio(completed, started);
        var qualityPenalty =
            (1.0 * disqualify + 0.8 * screenOut + 0.6 * quotaFull + 0.4 * partial) / Math.Max(1, started);
        var quality = Clamp01(1 - qualityPenalty);
        var approvalTrust = Ratio(rewardApproved, rewardApproved + rewardRejected);
        var speed = medianCompletionMinutes.HasValue
            ? Clamp01((60.0 - medianCompletionMinutes.Value) / 50.0)
            : 0.5;

        var weightSum = (double)(config.ParticipationWeight + config.CompletionWeight + config.QualityWeight +
            config.ApprovalTrustWeight + config.SpeedWeight);
        if (weightSum <= 0)
            weightSum = 1;

        var coreRaw = 100.0 * (
            (double)config.ParticipationWeight * participation +
            (double)config.CompletionWeight * completion +
            (double)config.QualityWeight * quality +
            (double)config.ApprovalTrustWeight * approvalTrust +
            (double)config.SpeedWeight * speed) / weightSum;

        var confidence = Math.Min(1.0, totalAssignments / (double)Math.Max(1, config.ConfidencePivot));
        var coreScore = (double)config.ScoreBaseline + (coreRaw - (double)config.ScoreBaseline) * confidence;

        var activityDays = activityStats?.ActiveDays30 ?? 0;
        var lastSeenAt = activityStats?.LastSeenAt;
        var activityMultiplier = CalculateActivityMultiplier(lastSeenAt, activityDays, nowUtc, config);
        var finalScore = decimal.Round((decimal)coreScore * activityMultiplier, 2);
        finalScore = decimal.Clamp(finalScore, 0m, 100m);
        var star = ToStar(finalScore, config);

        return new SubjectScoreSnapshot
        {
            SubjectId = subjectId,
            Score = finalScore,
            Star = star,
            CoreScore = decimal.Round((decimal)coreScore, 2),
            ActivityMultiplier = activityMultiplier,
            TotalAssignments = totalAssignments,
            Started = started,
            Completed = completed,
            NotStarted = notStarted,
            Partial = partial,
            Disqualify = disqualify,
            ScreenOut = screenOut,
            QuotaFull = quotaFull,
            RewardApproved = rewardApproved,
            RewardRejected = rewardRejected,
            MedianCompletionMinutes = medianCompletionMinutes,
            ActiveDays30 = activityDays,
            LastSeenAt = lastSeenAt,
            CalculatedAt = nowUtc
        };
    }

    private static int ToStar(decimal score, SubjectScoreConfigDto config)
    {
        if (score <= config.Star1Max) return 1;
        if (score <= config.Star2Max) return 2;
        if (score <= config.Star3Max) return 3;
        if (score <= config.Star4Max) return 4;
        return 5;
    }

    private static int? CalculateMedianCompletionMinutes(IEnumerable<ProjectAssignment> assignments)
    {
        var values = assignments
            .Where(a => a.Status == AssignmentStatus.Completed && a.CompletedAt.HasValue)
            .Select(a => Math.Max(0, (a.CompletedAt!.Value - a.AssignedAt).TotalMinutes))
            .OrderBy(x => x)
            .ToList();

        if (values.Count == 0)
            return null;

        var middle = values.Count / 2;
        if (values.Count % 2 == 1)
            return (int)Math.Round(values[middle], MidpointRounding.AwayFromZero);

        var median = (values[middle - 1] + values[middle]) / 2.0;
        return (int)Math.Round(median, MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateActivityMultiplier(
        DateTime? lastSeenAt,
        int activeDays30,
        DateTime nowUtc,
        SubjectScoreConfigDto config)
    {
        if (!lastSeenAt.HasValue)
            return config.NoTelemetryMultiplier;

        var lastSeenDays = (nowUtc - lastSeenAt.Value).TotalDays;
        if (lastSeenDays <= config.VeryActiveLastSeenDays && activeDays30 >= config.VeryActiveMinDays30)
            return config.VeryActiveMultiplier;
        if (lastSeenDays <= config.ActiveLastSeenDays)
            return config.ActiveMultiplier;
        if (lastSeenDays <= config.WarmLastSeenDays)
            return config.WarmMultiplier;
        if (lastSeenDays <= config.CoolingLastSeenDays)
            return config.CoolingMultiplier;
        return config.DormantMultiplier;
    }

    private async Task<SubjectScoreConfigDto> LoadConfigAsync()
    {
        var config = await configRepository.GetCurrentAsync();
        return config is null
            ? SubjectScoreConfigService.Default()
            : new SubjectScoreConfigDto(
                config.ParticipationWeight,
                config.CompletionWeight,
                config.QualityWeight,
                config.ApprovalTrustWeight,
                config.SpeedWeight,
                config.ConfidencePivot,
                config.ScoreBaseline,
                config.Star1Max,
                config.Star2Max,
                config.Star3Max,
                config.Star4Max,
                config.VeryActiveLastSeenDays,
                config.ActiveLastSeenDays,
                config.WarmLastSeenDays,
                config.CoolingLastSeenDays,
                config.VeryActiveMinDays30,
                config.VeryActiveMultiplier,
                config.ActiveMultiplier,
                config.WarmMultiplier,
                config.CoolingMultiplier,
                config.DormantMultiplier,
                config.NoTelemetryMultiplier);
    }

    private static double Ratio(int numerator, int denominator)
        => denominator <= 0 ? 0d : (double)numerator / denominator;

    private static double Clamp01(double value)
        => Math.Clamp(value, 0d, 1d);

    private static SubjectScoreDto MapToDto(SubjectScoreSnapshot x)
        => new(
            x.SubjectId,
            x.Score,
            x.Star,
            x.CoreScore,
            x.ActivityMultiplier,
            x.CalculatedAt,
            x.TotalAssignments,
            x.Started,
            x.Completed,
            x.NotStarted,
            x.Partial,
            x.Disqualify,
            x.ScreenOut,
            x.QuotaFull,
            x.RewardApproved,
            x.RewardRejected,
            x.MedianCompletionMinutes,
            x.ActiveDays30,
            x.LastSeenAt);
}
