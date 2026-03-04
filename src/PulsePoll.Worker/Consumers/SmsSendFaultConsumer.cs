using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Worker.Consumers;

public class SmsSendFaultConsumer(
    ISmsLogRepository smsLogRepository,
    ILogger<SmsSendFaultConsumer> logger) : IConsumer<Fault<SmsSendMessage>>
{
    public async Task Consume(ConsumeContext<Fault<SmsSendMessage>> context)
    {
        var msg = context.Message.Message;
        var err = context.Message.Exceptions.FirstOrDefault()?.Message ?? "SMS gönderimi başarısız";
        if (err.Length > 500)
            err = err[..500];

        var log = new SmsLog
        {
            PhoneNumber = msg.PhoneNumber,
            Message = msg.Message,
            SubjectId = msg.SubjectId,
            Source = msg.SentByAdminId.HasValue ? SmsSource.AdminBulk : SmsSource.System,
            DeliveryStatus = DeliveryStatus.Failed,
            ErrorMessage = err
        };
        log.SetCreated(msg.SentByAdminId ?? 0);
        await smsLogRepository.AddAsync(log);

        logger.LogWarning("SMS retry sonrası başarısız loglandı: {PhoneNumber}", msg.PhoneNumber);
    }
}
