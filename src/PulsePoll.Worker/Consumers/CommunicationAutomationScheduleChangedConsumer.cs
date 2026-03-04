using MassTransit;
using PulsePoll.Application.Messaging;
using PulsePoll.Worker.Services;

namespace PulsePoll.Worker.Consumers;

public class CommunicationAutomationScheduleChangedConsumer(
    ICommunicationAutomationJobScheduler scheduler,
    ILogger<CommunicationAutomationScheduleChangedConsumer> logger) : IConsumer<CommunicationAutomationScheduleChangedMessage>
{
    public async Task Consume(ConsumeContext<CommunicationAutomationScheduleChangedMessage> context)
    {
        var msg = context.Message;

        await scheduler.RefreshAsync(context.CancellationToken);

        logger.LogInformation(
            "Communication automation schedule updated via message. DailyRunTime={DailyRunTime} TimeZone={TimeZoneId} UpdatedBy={UpdatedBy}",
            msg.DailyRunTime,
            msg.TimeZoneId,
            msg.UpdatedByAdminId);
    }
}
