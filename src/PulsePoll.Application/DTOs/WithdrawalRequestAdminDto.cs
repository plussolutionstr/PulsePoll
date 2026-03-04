namespace PulsePoll.Application.DTOs;

public record WithdrawalRequestAdminDto(
    int Id,
    int SubjectId,
    string SubjectFullName,
    int BankAccountId,
    string BankName,
    string IbanLast4,
    string Iban,
    decimal Amount,
    decimal AmountTry,
    string UnitCode,
    string UnitLabel,
    decimal UnitTryMultiplier,
    string Status,
    DateTime CreatedAt,
    string? RejectionReason,
    DateTime? ProcessedAt);

public record RejectWithdrawalDto(string Reason);
