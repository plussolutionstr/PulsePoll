using Android.Gms.Extensions;
using Firebase.Messaging;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.Platforms.Android;

public class PushNotificationService : IPushNotificationService
{
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = Preferences.Default.Get<string?>("fcm_token", null);
            if (!string.IsNullOrEmpty(token))
                return token;

            var result = (Java.Lang.Object?)await FirebaseMessaging.Instance.GetToken();
            token = result?.ToString();

            if (!string.IsNullOrEmpty(token))
                Preferences.Default.Set("fcm_token", token);

            return token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] GetTokenAsync failed: {ex.Message}");
            return null;
        }
    }

    public async Task RegisterAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return;

            var apiClient = IPlatformApplication.Current!.Services.GetRequiredService<IPulsePollApiClient>();
            await apiClient.UpdateFcmTokenAsync(token);
            System.Diagnostics.Debug.WriteLine("[FCM] Token sent to server.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] RegisterAsync failed: {ex.Message}");
        }
    }
}
