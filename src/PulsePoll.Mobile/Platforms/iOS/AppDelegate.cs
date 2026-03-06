using Foundation;
using UIKit;

namespace PulsePoll.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        var bytes = new byte[deviceToken.Length];
        System.Runtime.InteropServices.Marshal.Copy(deviceToken.Bytes, bytes, 0, (int)deviceToken.Length);
        var token = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

        Preferences.Default.Set("fcm_token", token);
        System.Diagnostics.Debug.WriteLine($"[APNS] Device token: {token}");
    }

    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        System.Diagnostics.Debug.WriteLine($"[APNS] Registration failed: {error.LocalizedDescription}");
    }
}