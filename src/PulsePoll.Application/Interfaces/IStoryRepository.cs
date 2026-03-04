using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IStoryRepository
{
    Task<List<Story>> GetAllAsync();
    Task<List<Story>> GetActiveAsync();
    Task<Story?> GetByIdAsync(int id);
    Task AddAsync(Story story);
    Task UpdateAsync(Story story);
    Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> orders);
    Task DeleteAsync(Story story);
}
