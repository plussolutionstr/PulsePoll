using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public interface IPulsePollApiClient
{
    Task<bool> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<List<StoryModel>> GetStoriesAsync(CancellationToken ct = default);
    Task MarkStorySeenAsync(int storyId, CancellationToken ct = default);
    Task<List<NewsModel>> GetNewsAsync(CancellationToken ct = default);
    Task<List<SurveyModel>> GetProjectsAsync(CancellationToken ct = default);
    Task<SurveyModel?> GetProjectByIdAsync(int projectId, CancellationToken ct = default);
    Task<List<HistoryItemModel>> GetHistoryAsync(CancellationToken ct = default);
    Task<string> StartProjectAsync(int projectId, CancellationToken ct = default);
    Task SubmitProjectResultAsync(int projectId, string status, string? rawPayload = null, CancellationToken ct = default);
}
