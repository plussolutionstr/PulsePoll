namespace PulsePoll.Mobile.Services;

public interface IPushNotificationService
{
    Task<string?> GetTokenAsync();
    Task RegisterAsync();
}
