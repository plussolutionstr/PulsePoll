using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.DTOs;

public record PaymentBatchDto(
    int Id,
    string BatchNumber,
    PaymentBatchStatus Status,
    string StatusLabel,
    decimal TotalAmount,
    int ItemCount,
    string? Note,
    DateTime CreatedAt,
    DateTime? SentAt,
    DateTime? CompletedAt);

public record PaymentBatchDetailDto(
    int Id,
    string BatchNumber,
    PaymentBatchStatus Status,
    string StatusLabel,
    decimal TotalAmount,
    int ItemCount,
    string? Note,
    DateTime CreatedAt,
    DateTime? SentAt,
    DateTime? CompletedAt,
    List<PaymentBatchItemDto> Items);

public record PaymentBatchItemDto(
    int Id,
    int WithdrawalRequestId,
    int SubjectId,
    string SubjectFullName,
    string PhoneNumber,
    string BankName,
    string IbanLast4,
    string? IbanEncrypted,
    decimal Amount,
    decimal AmountTry,
    string UnitLabel,
    PaymentStatus Status,
    string StatusLabel,
    string? FailureReason,
    DateTime? ProcessedAt);

public record PaymentSettingDto(int Id, string Key, string Value, string? Description);

public record CreatePaymentBatchDto(List<int> WithdrawalRequestIds, string? Note);

public record UpdatePaymentItemStatusDto(PaymentStatus Status, string? FailureReason);

public record PaymentExportFileDto(
    string FileName,
    byte[] Content,
    string ContentType);
