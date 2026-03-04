using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface INewsRepository
{
    Task<List<News>> GetAllAsync();
    Task<List<News>> GetActiveAsync();
    Task<News?> GetByIdAsync(int id);
    Task AddAsync(News news);
    Task UpdateAsync(News news);
    Task ReorderAsync(IReadOnlyCollection<(int Id, int Order)> orders);
    Task DeleteAsync(News news);
}
