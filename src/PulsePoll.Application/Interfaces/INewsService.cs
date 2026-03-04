using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface INewsService
{
    Task<List<NewsDto>> GetAllAsync();
    Task<List<NewsDto>> GetActiveAsync();
    Task<NewsDto> CreateAsync(CreateNewsDto dto);
    Task UpdateAsync(int id, CreateNewsDto dto);
    Task ReorderAsync(IReadOnlyCollection<OrderUpdateDto> orders);
    Task DeleteAsync(int id);
}
