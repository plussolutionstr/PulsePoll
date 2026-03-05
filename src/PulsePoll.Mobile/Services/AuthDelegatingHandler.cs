using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PulsePoll.Mobile.ApiModels;

namespace PulsePoll.Mobile.Services;

public sealed class AuthDelegatingHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AttachTokenAsync(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        if (IsAuthEndpoint(request.RequestUri))
            return response;

        var refreshed = await TryRefreshTokenAsync(cancellationToken);
        if (!refreshed)
            return response;

        response.Dispose();
        var retry = await CloneRequestAsync(request);
        await AttachTokenAsync(retry);
        return await base.SendAsync(retry, cancellationToken);
    }

    private async Task AttachTokenAsync(HttpRequestMessage request)
    {
        var token = await tokenProvider.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken ct)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            var refreshToken = await tokenProvider.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
            {
                Content = JsonContent.Create(refreshToken, options: JsonOptions)
            };

            var response = await base.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                await tokenProvider.ClearTokensAsync();
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResultDto>>(JsonOptions, ct);
            if (result is not { Success: true } || result.Data is null)
            {
                await tokenProvider.ClearTokensAsync();
                return false;
            }

            await tokenProvider.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static bool IsAuthEndpoint(Uri? uri)
    {
        if (uri is null) return false;
        var path = uri.AbsolutePath;
        return path.Contains("/auth/login", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/auth/refresh", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/auth/register", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        if (original.Content is not null)
        {
            var content = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            if (original.Content.Headers.ContentType is not null)
                clone.Content.Headers.ContentType = original.Content.Headers.ContentType;
        }

        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}
