using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SubjectScoreSnapshotRepository(AppDbContext db) : ISubjectScoreSnapshotRepository
{
    public Task<List<SubjectScoreSnapshot>> GetBySubjectIdsAsync(IEnumerable<int> subjectIds)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            return Task.FromResult(new List<SubjectScoreSnapshot>());

        return db.Set<SubjectScoreSnapshot>()
            .Where(x => ids.Contains(x.SubjectId))
            .ToListAsync();
    }

    public async Task UpsertManyAsync(IEnumerable<SubjectScoreSnapshot> snapshots, int actorId = 0)
    {
        var incoming = snapshots.ToList();
        if (incoming.Count == 0)
            return;

        var ids = incoming.Select(x => x.SubjectId).Distinct().ToArray();
        var existing = await db.Set<SubjectScoreSnapshot>()
            .Where(x => ids.Contains(x.SubjectId))
            .ToDictionaryAsync(x => x.SubjectId);

        foreach (var item in incoming)
        {
            if (existing.TryGetValue(item.SubjectId, out var row))
            {
                row.Score = item.Score;
                row.Star = item.Star;
                row.CoreScore = item.CoreScore;
                row.ActivityMultiplier = item.ActivityMultiplier;
                row.TotalAssignments = item.TotalAssignments;
                row.Started = item.Started;
                row.Completed = item.Completed;
                row.NotStarted = item.NotStarted;
                row.Partial = item.Partial;
                row.Disqualify = item.Disqualify;
                row.ScreenOut = item.ScreenOut;
                row.QuotaFull = item.QuotaFull;
                row.RewardApproved = item.RewardApproved;
                row.RewardRejected = item.RewardRejected;
                row.MedianCompletionMinutes = item.MedianCompletionMinutes;
                row.ActiveDays30 = item.ActiveDays30;
                row.LastSeenAt = item.LastSeenAt;
                row.CalculatedAt = item.CalculatedAt;
                row.SetUpdated(actorId);
            }
            else
            {
                item.SetCreated(actorId);
                db.Set<SubjectScoreSnapshot>().Add(item);
            }
        }

        await db.SaveChangesAsync();
    }
}

