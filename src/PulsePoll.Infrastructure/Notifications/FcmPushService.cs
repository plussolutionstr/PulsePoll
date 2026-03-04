using FirebaseAdmin.Messaging;

namespace PulsePoll.Infrastructure.Notifications;

public class FcmPushService
{
    public async Task SendAsync(string fcmToken, string title, string body)
    {
        var message = new Message
        {
            Token = fcmToken,
            Notification = new Notification { Title = title, Body = body },
            Android = new AndroidConfig { Priority = Priority.High }
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task SendMulticastAsync(IEnumerable<string> fcmTokens, string title, string body)
    {
        var message = new MulticastMessage
        {
            Tokens = fcmTokens.ToList(),
            Notification = new Notification { Title = title, Body = body }
        };

        await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }
}
