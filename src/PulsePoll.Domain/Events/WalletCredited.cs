namespace PulsePoll.Domain.Events;

public record WalletCredited(
    Guid UserId,
    decimal Amount,
    string ReferenceId,
    DateTime CreditedAt);
