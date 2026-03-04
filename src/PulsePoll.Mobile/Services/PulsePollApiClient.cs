using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public sealed class PulsePollApiClient : IPulsePollApiClient
{
    private readonly HttpClient _http;
    private readonly ITokenProvider _tokenProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PulsePollApiClient(HttpClient http, ITokenProvider tokenProvider)
    {
        _http = http;
        _tokenProvider = tokenProvider;
    }

    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var payload = new { email, password };
        var response = await _http.PostAsJsonAsync("api/auth/login", payload, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResultDto>>(JsonOptions, ct);
        if (result is not { Success: true } || result.Data is null)
            return false;

        await _tokenProvider.SetTokenAsync(result.Data.AccessToken);
        return true;
    }

    public async Task<List<StoryModel>> GetStoriesAsync(CancellationToken ct = default)
    {
        var response = await GetAsync<List<StoryApiDto>>("api/stories", ct);
        return response?.Select(s => s.ToModel()).ToList() ?? [];
    }

    public async Task<List<NewsModel>> GetNewsAsync(CancellationToken ct = default)
    {
        var response = await GetAsync<List<NewsApiDto>>("api/news", ct);
        return response?.Select(n => n.ToModel()).ToList() ?? [];
    }

    public async Task<List<SurveyModel>> GetProjectsAsync(CancellationToken ct = default)
    {
        var response = await GetAsync<List<ProjectApiDto>>("api/projects", ct);
        return response?
            .Where(p => p.Status == ProjectStatus.Active)
            .Select(p => p.ToModel())
            .ToList() ?? [];
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        await SetAuthHeaderAsync();
        var result = await _http.GetFromJsonAsync<ApiResponse<T>>(path, JsonOptions, ct);
        return result is { Success: true } ? result.Data : default;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenProvider.GetAccessTokenAsync();
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }
}
