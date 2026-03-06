using PulsePoll.Mobile.Services;
using UIKit;
using UserNotifications;

namespace PulsePoll.Mobile.Platforms.iOS;

public class PushNotificationService : IPushNotificationService
{
    public Task<string?> GetTokenAsync()
    {
        var token = Preferences.Default.Get<string?>("fcm_token", null);
        return Task.FromResult(token);
    }

    public async Task RegisterAsync()
    {
        var authStatus = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound);

        if (authStatus.Item1)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            });
        }

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return;

        try
        {
            var apiClient = IPlatformApplication.Current!.Services.GetRequiredService<IPulsePollApiClient>();
            await apiClient.UpdateFcmTokenAsync(token);
            System.Diagnostics.Debug.WriteLine("[FCM] iOS token sent to server.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] iOS RegisterAsync failed: {ex.Message}");
        }
    }
}
