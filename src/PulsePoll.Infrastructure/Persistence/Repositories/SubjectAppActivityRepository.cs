using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SubjectAppActivityRepository(AppDbContext db) : ISubjectAppActivityRepository
{
    public async Task AddAsync(SubjectAppActivity activity)
    {
        db.SubjectAppActivities.Add(activity);
        await db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, SubjectActivityStats>> GetStatsBySubjectIdsAsync(IEnumerable<int> subjectIds, DateTime sinceUtc)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, SubjectActivityStats>();

        var recentRows = await db.SubjectAppActivities
            .AsNoTracking()
            .Where(x => ids.Contains(x.SubjectId) && x.OccurredAt >= sinceUtc)
            .Select(x => new { x.SubjectId, x.OccurredAt })
            .ToListAsync();

        var activeDays = recentRows
            .GroupBy(x => x.SubjectId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => DateOnly.FromDateTime(x.OccurredAt.Date)).Distinct().Count());

        var lastSeenMap = await db.SubjectAppActivities
            .AsNoTracking()
            .Where(x => ids.Contains(x.SubjectId))
            .GroupBy(x => x.SubjectId)
            .Select(g => new { SubjectId = g.Key, LastSeenAt = g.Max(x => x.OccurredAt) })
            .ToDictionaryAsync(x => x.SubjectId, x => (DateTime?)x.LastSeenAt);

        var result = new Dictionary<int, SubjectActivityStats>(ids.Length);
        foreach (var subjectId in ids)
        {
            activeDays.TryGetValue(subjectId, out var days);
            lastSeenMap.TryGetValue(subjectId, out var lastSeenAt);
            result[subjectId] = new SubjectActivityStats(subjectId, days, lastSeenAt);
        }

        return result;
    }
}

