using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Worker.Consumers;

public class SmsSendConsumer(
    ISmsService smsService,
    ISmsLogRepository smsLogRepository,
    ILogger<SmsSendConsumer> logger) : IConsumer<SmsSendMessage>
{
    public async Task Consume(ConsumeContext<SmsSendMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("SMS gönderiliyor: {PhoneNumber}", msg.PhoneNumber);

        await smsService.SendAsync(msg.PhoneNumber, msg.Message,
            subjectId: msg.SubjectId, sentByAdminId: msg.SentByAdminId);

        var log = new SmsLog
        {
            PhoneNumber = msg.PhoneNumber,
            Message = msg.Message,
            SubjectId = msg.SubjectId,
            Source = msg.SentByAdminId.HasValue ? SmsSource.AdminBulk : SmsSource.System,
            DeliveryStatus = DeliveryStatus.Sent
        };
        log.SetCreated(msg.SentByAdminId ?? 0);
        await smsLogRepository.AddAsync(log);

        logger.LogInformation("SMS gönderildi ve loglandı: {PhoneNumber}", msg.PhoneNumber);
    }
}
