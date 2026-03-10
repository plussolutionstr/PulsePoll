using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SubjectAppActivityRepository(AppDbContext db) : ISubjectAppActivityRepository
{
    public async Task<SubjectAppActivity?> GetBySubjectAndDateAsync(int subjectId, DateOnly date, CancellationToken ct)
    {
        return await db.SubjectAppActivities
            .FirstOrDefaultAsync(x => x.SubjectId == subjectId && x.ActivityDate == date, ct);
    }

    public async Task AddAsync(SubjectAppActivity activity, CancellationToken ct)
    {
        db.SubjectAppActivities.Add(activity);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SubjectAppActivity activity, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task<Dictionary<int, SubjectActivityStats>> GetStatsBySubjectIdsAsync(
        IEnumerable<int> subjectIds, DateTime sinceUtc)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, SubjectActivityStats>();

        var stats = await db.SubjectAppActivities
            .AsNoTracking()
            .Where(x => ids.Contains(x.SubjectId))
            .GroupBy(x => x.SubjectId)
            .Select(g => new
            {
                SubjectId = g.Key,
                ActiveDays = g.Count(x => x.ActivityDate >= DateOnly.FromDateTime(sinceUtc)),
                LastSeenAt = g.Max(x => x.LastSeenAt)
            })
            .ToDictionaryAsync(
                x => x.SubjectId,
                x => new SubjectActivityStats(x.SubjectId, x.ActiveDays, x.LastSeenAt));

        return stats;
    }
}
