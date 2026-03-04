using MassTransit;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Worker.Consumers;

public class WithdrawalRequestedConsumer(
    IWithdrawalRequestRepository withdrawalRequestRepository,
    ILogger<WithdrawalRequestedConsumer> logger) : IConsumer<WithdrawalRequestedMessage>
{
    public async Task Consume(ConsumeContext<WithdrawalRequestedMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation("Para çekimi talebi alındı: SubjectId={SubjectId} Amount={Amount} TransactionId={TransactionId}",
            msg.SubjectId, msg.Amount, msg.WalletTransactionId);

        // İdempotency: aynı transaction için kayıt zaten varsa atla
        var existing = await withdrawalRequestRepository.GetByTransactionIdAsync(msg.WalletTransactionId);
        if (existing is not null)
        {
            logger.LogWarning("Para çekimi talebi zaten mevcut, atlanıyor: TransactionId={TransactionId}", msg.WalletTransactionId);
            return;
        }

        var request = new WithdrawalRequest
        {
            SubjectId           = msg.SubjectId,
            BankAccountId       = msg.BankAccountId,
            WalletTransactionId = msg.WalletTransactionId,
            Amount              = msg.Amount,
            AmountTry           = msg.AmountTry,
            UnitCode            = msg.UnitCode,
            UnitLabel           = msg.UnitLabel,
            UnitTryMultiplier   = msg.UnitTryMultiplier,
            Status              = ApprovalStatus.Pending
        };
        request.SetCreated(msg.SubjectId);

        await withdrawalRequestRepository.AddAsync(request);

        logger.LogInformation("WithdrawalRequest oluşturuldu: Id={Id} SubjectId={SubjectId} Amount={Amount}",
            request.Id, msg.SubjectId, msg.Amount);
    }
}
