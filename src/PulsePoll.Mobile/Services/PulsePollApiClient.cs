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

    public async Task<bool> LoginAsync(string phoneNumber, string password, CancellationToken ct = default)
    {
        var payload = new { phoneNumber, password };
        var response = await _http.PostAsJsonAsync("api/auth/login", payload, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResultDto>>(JsonOptions, ct);
        if (result is not { Success: true } || result.Data is null)
            return false;

        await _tokenProvider.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
        return true;
    }

    public async Task SendOtpAsync(string phoneNumber, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/send-otp", new { phoneNumber }, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task<string> VerifyOtpAsync(string phoneNumber, string otp, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/verify-otp", new { phoneNumber, otp }, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OtpVerifiedApiDto>>(JsonOptions, ct);
        return result?.Data?.RegistrationToken ?? throw new InvalidOperationException("OTP doğrulanamadı.");
    }

    public async Task RegisterAsync(object dto, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", dto, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var response = await _http.PostAsync("api/auth/logout", null, ct);
        await _tokenProvider.ClearTokensAsync();
    }

    public async Task<bool> TryRefreshSessionAsync(CancellationToken ct = default)
    {
        var refreshToken = await _tokenProvider.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/refresh", refreshToken, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                await _tokenProvider.ClearTokensAsync();
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResultDto>>(JsonOptions, ct);
            if (result is not { Success: true } || result.Data is null)
            {
                await _tokenProvider.ClearTokensAsync();
                return false;
            }

            await _tokenProvider.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task SendPasswordResetOtpAsync(string phoneNumber, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/send-password-reset-otp", new { phoneNumber }, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/reset-password", new { phoneNumber, otp, newPassword }, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task<List<StoryModel>> GetStoriesAsync(CancellationToken ct = default)
    {
        var response = await GetAsync<List<StoryApiDto>>("api/stories", ct);
        return response?.Select(s => s.ToModel()).ToList() ?? [];
    }

    public async Task MarkStorySeenAsync(int storyId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/stories/{storyId}/seen", content: null, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object?>>(JsonOptions, ct);
        if (result is not { Success: true })
            throw new HttpRequestException("Story seen işareti API tarafından reddedildi.");
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
            .Where(p => p.Status == ProjectStatus.Active &&
                        p.AssignmentStatus is null or AssignmentStatus.NotStarted or AssignmentStatus.Partial)
            .Select(p => p.ToModel())
            .ToList() ?? [];
    }

    public async Task<SurveyModel?> GetProjectByIdAsync(int projectId, CancellationToken ct = default)
    {
        var response = await GetAsync<ProjectApiDto>($"api/projects/{projectId}", ct);
        return response?.ToModel();
    }

    public async Task<List<HistoryItemModel>> GetHistoryAsync(CancellationToken ct = default)
    {
        var response = await GetAsync<List<ProjectHistoryApiDto>>("api/profile/projects/history", ct);
        return response?.Select(h => h.ToModel()).ToList() ?? [];
    }

    public Task<WalletApiDto?> GetWalletAsync(CancellationToken ct = default)
        => GetAsync<WalletApiDto>("api/wallet", ct);

    public async Task<List<WalletTransactionApiDto>> GetWalletTransactionsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await GetPagedAsync<WalletTransactionApiDto>($"api/wallet/transactions?page={page}&pageSize={pageSize}", ct);
        return result ?? [];
    }

    public async Task<List<BankAccountApiDto>> GetBankAccountsAsync(CancellationToken ct = default)
    {
        var result = await GetAsync<List<BankAccountApiDto>>("api/wallet/banks", ct);
        return result ?? [];
    }

    public async Task<List<BankOptionApiDto>> GetAvailableBanksAsync(CancellationToken ct = default)
    {
        var result = await GetAsync<List<BankOptionApiDto>>("api/wallet/bank-options", ct);
        return result ?? [];
    }

    public async Task AddBankAccountAsync(int bankId, string iban, CancellationToken ct = default)
    {
        var payload = new AddBankAccountApiRequest(bankId, iban);
        var response = await _http.PostAsJsonAsync("api/wallet/banks", payload, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task UpdateBankAccountAsync(int bankAccountId, int bankId, string iban, CancellationToken ct = default)
    {
        var payload = new UpdateBankAccountApiRequest(bankId, iban);
        var response = await _http.PutAsJsonAsync($"api/wallet/banks/{bankAccountId}", payload, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task DeleteBankAccountAsync(int bankAccountId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"api/wallet/banks/{bankAccountId}", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task RequestWithdrawalAsync(decimal amount, int bankAccountId, CancellationToken ct = default)
    {
        var payload = new WithdrawalRequestApiRequest(amount, bankAccountId);
        var response = await _http.PostAsJsonAsync("api/wallet/withdraw", payload, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task<string> StartProjectAsync(int projectId, CancellationToken ct = default)
    {
        var response = await _http.PostAsync($"api/projects/{projectId}/start", content: null, ct);
        await EnsureSuccessOrThrowAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StartProjectApiDto>>(JsonOptions, ct);
        if (result is not { Success: true } || string.IsNullOrWhiteSpace(result.Data?.Url))
            throw new HttpRequestException("Anket başlatma URL'si alınamadı.");

        return result.Data.Url;
    }

    public Task<ProfileApiDto?> GetProfileAsync(CancellationToken ct = default)
        => GetAsync<ProfileApiDto>("api/profile", ct);

    public async Task<ProfileApiDto?> UpdateProfileAsync(object dto, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync("api/profile", dto, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProfileApiDto>>(JsonOptions, ct);
        return result is { Success: true } ? result.Data : null;
    }

    public async Task<string?> UploadProfilePhotoAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        var response = await _http.PostAsync("api/profile/photo", content, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PhotoUploadResultDto>>(JsonOptions, ct);
        return result is { Success: true } ? result.Data?.Url : null;
    }

    public async Task<List<LookupItemDto>> GetCitiesAsync(CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>("api/lookups/cities", ct) ?? [];

    public async Task<List<LookupItemDto>> GetDistrictsAsync(int cityId, CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>($"api/lookups/cities/{cityId}/districts", ct) ?? [];

    public async Task<List<LookupItemDto>> GetProfessionsAsync(CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>("api/lookups/professions", ct) ?? [];

    public async Task<List<LookupItemDto>> GetEducationLevelsAsync(CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>("api/lookups/education-levels", ct) ?? [];

    public async Task<List<LookupItemDto>> GetRegisterCitiesAsync(CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>("api/auth/register/cities", ct) ?? [];

    public async Task<List<LookupItemDto>> GetRegisterDistrictsAsync(int cityId, CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>($"api/auth/register/cities/{cityId}/districts", ct) ?? [];

    public async Task<List<LookupItemDto>> GetRegisterProfessionsAsync(CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>("api/auth/register/professions", ct) ?? [];

    public async Task<List<LookupItemDto>> GetRegisterEducationLevelsAsync(CancellationToken ct = default)
        => await GetAsync<List<LookupItemDto>>("api/auth/register/education-levels", ct) ?? [];

    public async Task<List<BankOptionApiDto>> GetRegisterBankOptionsAsync(CancellationToken ct = default)
        => await GetAsync<List<BankOptionApiDto>>("api/auth/register/bank-options", ct) ?? [];

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<T>>(path, JsonOptions, ct);
        return result is { Success: true } ? result.Data : default;
    }

    private async Task<List<T>?> GetPagedAsync<T>(string path, CancellationToken ct)
    {
        var result = await _http.GetFromJsonAsync<PagedApiResponse<T>>(path, JsonOptions, ct);
        return result is { Success: true } ? result.Data : default;
    }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        string? message = null;
        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object?>>(JsonOptions, ct);
            message = errorResponse?.Error?.Message;
        }
        catch
        {
            // ignore deserialization errors and fallback to status line
        }

        if (string.IsNullOrWhiteSpace(message))
            message = $"İstek başarısız oldu ({(int)response.StatusCode}).";

        throw new HttpRequestException(message, null, response.StatusCode);
    }

    public async Task<AppContentApiDto?> GetAppContentAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/app-content", ct);
        await EnsureSuccessOrThrowAsync(response, ct);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<AppContentApiDto>>(JsonOptions, ct);
        return envelope?.Data;
    }

    public async Task UpdateFcmTokenAsync(string fcmToken, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync("api/profile/fcm-token", fcmToken, JsonOptions, ct);
        await EnsureSuccessOrThrowAsync(response, ct);
    }

    public async Task PingAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("api/projects", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]?> GetImageBytesAsync(string url, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}
