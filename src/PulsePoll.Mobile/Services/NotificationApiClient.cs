using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PulsePoll.Mobile.ApiModels;
using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public sealed class NotificationApiClient : INotificationApiClient
{
    private readonly HttpClient _http;
    private readonly ITokenProvider _tokenProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public NotificationApiClient(HttpClient http, ITokenProvider tokenProvider)
    {
        _http = http;
        _tokenProvider = tokenProvider;
    }

    public async Task<List<NotificationModel>> GetNotificationsAsync(CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _http.GetFromJsonAsync<ApiResponse<List<NotificationApiDto>>>("api/notifications", JsonOptions, ct);
        if (response is not { Success: true } || response.Data is null)
            return [];

        return response.Data
            .Select(n => new NotificationModel(
                n.Id,
                n.Type ?? "system",
                n.Title,
                n.Body,
                n.CreatedAt,
                n.IsRead))
            .OrderByDescending(n => n.Date)
            .ToList();
    }

    public async Task MarkAllReadAsync(CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsync("api/notifications/read-all", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkOneReadAsync(int notificationId, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _http.PutAsync($"api/notifications/{notificationId}/read", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int notificationId, CancellationToken ct = default)
    {
        await SetAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/notifications/{notificationId}", ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenProvider.GetAccessTokenAsync();
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }
}
