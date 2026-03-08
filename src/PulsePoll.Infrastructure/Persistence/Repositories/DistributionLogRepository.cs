using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class DistributionLogRepository(AppDbContext db) : IDistributionLogRepository
{
    public async Task AddAsync(DistributionLog log)
    {
        db.DistributionLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public Task<List<DistributionLog>> GetByProjectAsync(int projectId)
        => db.DistributionLogs
             .Where(d => d.ProjectId == projectId)
             .OrderByDescending(d => d.RunDate)
             .ThenByDescending(d => d.RunTime)
             .ToListAsync();

    public Task<int> GetTodayDistributedCountAsync(int projectId, DateOnly today)
        => db.DistributionLogs
             .Where(d => d.ProjectId == projectId && d.RunDate == today)
             .SumAsync(d => d.DistributedCount);
}
