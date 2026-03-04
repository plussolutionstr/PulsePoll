using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;

namespace PulsePoll.Worker.Consumers;

public class WalletCreditConsumer(
    ILogger<WalletCreditConsumer> logger,
    IWalletService walletService) : IConsumer<WalletCreditMessage>
{
    public async Task Consume(ConsumeContext<WalletCreditMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("Cüzdan kredisi işleniyor: SubjectId={SubjectId} Amount={Amount}",
            msg.SubjectId, msg.Amount);

        await walletService.CreditAsync(msg.SubjectId, msg.Amount, msg.ReferenceId, msg.Description);

        logger.LogInformation("Cüzdan kredisi tamamlandı: SubjectId={SubjectId}", msg.SubjectId);
    }
}
