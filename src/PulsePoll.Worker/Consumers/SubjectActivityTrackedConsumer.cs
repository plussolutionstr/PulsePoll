using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;

namespace PulsePoll.Worker.Consumers;

public class SubjectActivityTrackedConsumer(
    ILogger<SubjectActivityTrackedConsumer> logger,
    ISubjectTelemetryService telemetryService) : IConsumer<SubjectActivityTrackedMessage>
{
    public async Task Consume(ConsumeContext<SubjectActivityTrackedMessage> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Denek aktivitesi işleniyor: SubjectId={SubjectId} Type={Type}",
            msg.SubjectId, msg.ActivityType);

        await telemetryService.ProcessActivityAsync(
            msg.SubjectId,
            msg.ActivityType,
            msg.Platform,
            msg.AppVersion,
            msg.DeviceId,
            msg.OccurredAt,
            context.CancellationToken);

        logger.LogInformation(
            "Denek aktivitesi tamamlandı: SubjectId={SubjectId}",
            msg.SubjectId);
    }
}
