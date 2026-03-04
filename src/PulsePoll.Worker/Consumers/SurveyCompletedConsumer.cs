using MassTransit;
using PulsePoll.Application.Messaging;

namespace PulsePoll.Worker.Consumers;

public class SurveyCompletedConsumer(
    ILogger<SurveyCompletedConsumer> logger) : IConsumer<SurveyCompletedMessage>
{
    public async Task Consume(ConsumeContext<SurveyCompletedMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("Anket tamamlandı, ödül admin onayı bekliyor: ProjectId={ProjectId} SubjectId={SubjectId} Reward={Reward}",
            msg.ProjectId, msg.SubjectId, msg.RewardAmount);
        await Task.CompletedTask;
    }
}
