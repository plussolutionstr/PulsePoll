namespace PulsePoll.Mobile.Services;

public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync();
    Task SetTokenAsync(string token);
    Task ClearTokenAsync();
}
