using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Enums;
using PulsePoll.Infrastructure.Notifications;

namespace PulsePoll.Worker.Consumers;

public class NotificationSendConsumer(
    ILogger<NotificationSendConsumer> logger,
    FcmPushService fcmService,
    INotificationRepository notificationRepository) : IConsumer<NotificationSendMessage>
{
    public async Task Consume(ConsumeContext<NotificationSendMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("Push bildirimi gönderiliyor: SubjectId={SubjectId}", msg.SubjectId);
        try
        {
            await fcmService.SendAsync(msg.FcmToken, msg.Title, msg.Body);
            await notificationRepository.UpdateDeliveryStatusAsync(msg.NotificationId, DeliveryStatus.Sent, null);
            logger.LogInformation("Push bildirimi gönderildi: SubjectId={SubjectId}", msg.SubjectId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Push gönderilemedi: SubjectId={SubjectId}", msg.SubjectId);
            throw;
        }
    }
}
