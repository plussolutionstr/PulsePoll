namespace PulsePoll.Mobile.Services;

public sealed class DevTokenProvider : ITokenProvider
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";

    public Task<string?> GetAccessTokenAsync()
    {
        var token = Preferences.Default.Get<string?>(AccessTokenKey, null);
        return Task.FromResult(token);
    }

    public Task<string?> GetRefreshTokenAsync()
    {
        var token = Preferences.Default.Get<string?>(RefreshTokenKey, null);
        return Task.FromResult(token);
    }

    public Task SetTokensAsync(string accessToken, string refreshToken)
    {
        Preferences.Default.Set(AccessTokenKey, accessToken);
        Preferences.Default.Set(RefreshTokenKey, refreshToken);
        return Task.CompletedTask;
    }

    public Task SetAccessTokenAsync(string accessToken)
    {
        Preferences.Default.Set(AccessTokenKey, accessToken);
        return Task.CompletedTask;
    }

    public Task ClearTokensAsync()
    {
        Preferences.Default.Remove(AccessTokenKey);
        Preferences.Default.Remove(RefreshTokenKey);
        return Task.CompletedTask;
    }

    public bool HasTokens()
    {
        var refresh = Preferences.Default.Get<string?>(RefreshTokenKey, null);
        return !string.IsNullOrEmpty(refresh);
    }
}
