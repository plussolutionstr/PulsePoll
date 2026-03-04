using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Worker.Consumers;

public class NotificationSendFaultConsumer(
    INotificationRepository notificationRepository,
    ILogger<NotificationSendFaultConsumer> logger) : IConsumer<Fault<NotificationSendMessage>>
{
    public async Task Consume(ConsumeContext<Fault<NotificationSendMessage>> context)
    {
        var msg = context.Message.Message;
        var err = context.Message.Exceptions.FirstOrDefault()?.Message ?? "Push gönderimi başarısız";
        if (err.Length > 500)
            err = err[..500];

        await notificationRepository.UpdateDeliveryStatusAsync(msg.NotificationId, DeliveryStatus.Failed, err);
        logger.LogWarning("Push retry sonrası başarısız loglandı: NotificationId={NotificationId}", msg.NotificationId);
    }
}
