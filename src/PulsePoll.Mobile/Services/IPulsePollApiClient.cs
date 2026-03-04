using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public interface IPulsePollApiClient
{
    Task<List<StoryModel>> GetStoriesAsync(CancellationToken ct = default);
    Task<List<NewsModel>> GetNewsAsync(CancellationToken ct = default);
    Task<List<SurveyModel>> GetProjectsAsync(CancellationToken ct = default);
}
