using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record WalletDto(
    int SubjectId,
    decimal Balance,
    decimal TotalEarned,
    decimal PendingBalance,
    decimal RejectedBalance,
    DateTime UpdatedAt,
    string UnitCode = "TRY",
    string UnitLabel = "TL",
    decimal UnitTryMultiplier = 1m);

public record WalletTransactionDto(
    int Id,
    decimal Amount,
    WalletTransactionType Type,
    string? Description,
    DateTime CreatedAt,
    bool IsManual,
    string UnitLabel = "TL");

public record WalletLedgerDto(
    int Id,
    decimal Amount,
    WalletTransactionType Type,
    string? Description,
    DateTime CreatedAt,
    decimal CumulativeBalance,
    bool IsManual,
    string UnitLabel = "TL");

public record WithdrawalRequestDto(
    decimal Amount,
    int BankAccountId);

public record BankAccountDto(
    int Id,
    string BankName,
    string IbanLast4,
    bool IsDefault,
    string? ThumbnailImageUrl,
    string? LogoImageUrl);

public record AddBankAccountDto(
    int BankId,
    string Iban);

public record AvailableBankDto(
    int Id,
    string Name,
    string? Code,
    string? ThumbnailImageUrl,
    string? LogoImageUrl);
