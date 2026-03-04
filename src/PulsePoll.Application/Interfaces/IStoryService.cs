using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IStoryService
{
    Task<List<StoryDto>> GetAllAsync();
    Task<IEnumerable<StoryDto>> GetActiveStoriesAsync();
    Task<StoryDto> CreateAsync(CreateStoryDto dto);
    Task UpdateAsync(int id, CreateStoryDto dto);
    Task ReorderAsync(IReadOnlyCollection<OrderUpdateDto> orders);
    Task DeleteAsync(int id);
}
