using Android.App;
using Android.OS;
using Firebase.Messaging;

namespace PulsePoll.Mobile.Platforms.Android;

[Service(Exported = true)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class PulsePollFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);
        Preferences.Default.Set("fcm_token", token);
        System.Diagnostics.Debug.WriteLine($"[FCM] New token: {token}");
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var notification = message.GetNotification();
        var data = message.Data;
        var title = notification?.Title ?? (data.TryGetValue("title", out var t) ? t : "PulsePoll");
        var body = notification?.Body ?? (data.TryGetValue("body", out var b) ? b : "");

        ShowLocalNotification(title, body);
    }

    private void ShowLocalNotification(string title, string body)
    {
        const string channelId = "pulsepoll_default";

        var context = global::Android.App.Application.Context;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
#pragma warning disable CA1416
            var channel = new NotificationChannel(channelId, "PulsePoll Bildirimleri", NotificationImportance.High);
            var manager = (NotificationManager?)context.GetSystemService(NotificationService);
            manager?.CreateNotificationChannel(channel);
#pragma warning restore CA1416
        }

        var intent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName!);
        var pendingIntent = PendingIntent.GetActivity(context, 0, intent,
            Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.Immutable : 0);

#pragma warning disable CA1416
        var builder = new Notification.Builder(context, channelId)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent);
#pragma warning restore CA1416

        var notificationManager = (NotificationManager?)context.GetSystemService(NotificationService);
        notificationManager?.Notify(DateTime.Now.Millisecond, builder?.Build());
    }
}
