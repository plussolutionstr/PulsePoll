namespace PulsePoll.Mobile.Services;

public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SetTokensAsync(string accessToken, string refreshToken);
    Task SetAccessTokenAsync(string accessToken);
    Task ClearTokensAsync();
    bool HasTokens();
}
