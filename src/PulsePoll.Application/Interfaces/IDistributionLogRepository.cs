using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IDistributionLogRepository
{
    Task AddAsync(DistributionLog log);
    Task<List<DistributionLog>> GetByProjectAsync(int projectId);
    Task<int> GetTodayDistributedCountAsync(int projectId, DateOnly today);
}
