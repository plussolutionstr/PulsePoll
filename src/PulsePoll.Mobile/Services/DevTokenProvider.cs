namespace PulsePoll.Mobile.Services;

public sealed class DevTokenProvider : ITokenProvider
{
    private const string TokenKey = "auth_access_token";

    public Task<string?> GetAccessTokenAsync()
    {
        var token = Preferences.Default.Get<string?>(TokenKey, null);
        return Task.FromResult(token);
    }

    public Task SetTokenAsync(string token)
    {
        Preferences.Default.Set(TokenKey, token);
        return Task.CompletedTask;
    }

    public Task ClearTokenAsync()
    {
        Preferences.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }
}
