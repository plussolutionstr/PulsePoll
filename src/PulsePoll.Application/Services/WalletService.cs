using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.Constants;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Application.Models;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class WalletService(
    IWalletRepository walletRepository,
    IProjectRepository projectRepository,
    IWithdrawalRequestRepository withdrawalRequestRepository,
    IPaymentSettingRepository paymentSettingRepository,
    INotificationRepository notificationRepository,
    ISubjectRepository subjectRepository,
    IRewardUnitConfigService rewardUnitConfigService,
    IMessagePublisher publisher,
    IValidator<WithdrawalRequestDto> withdrawalValidator,
    IValidator<AddBankAccountDto> bankAccountValidator,
    ILogger<WalletService> logger) : IWalletService
{
    public async Task<WalletDto> GetBySubjectIdAsync(int subjectId)
    {
        var wallet = await walletRepository.GetBySubjectIdAsync(subjectId)
            ?? throw new NotFoundException("Cüzdan");

        var assignments = await projectRepository.GetSubjectAssignmentsAsync(subjectId);
        var pendingBalance = assignments
            .Where(a => IsRewardEligibleStatus(a.Status) && a.RewardStatus is RewardStatus.Pending or RewardStatus.None)
            .Sum(a => a.EarnedAmount ?? 0m);
        var rejectedBalance = assignments
            .Where(a => IsRewardEligibleStatus(a.Status) && a.RewardStatus == RewardStatus.Rejected)
            .Sum(a => a.EarnedAmount ?? 0m);
        var rewardUnit = await rewardUnitConfigService.GetAsync();

        return new WalletDto(
            wallet.SubjectId,
            wallet.Balance,
            wallet.TotalEarned,
            pendingBalance,
            rejectedBalance,
            wallet.UpdatedAt ?? wallet.CreatedAt,
            rewardUnit.UnitCode,
            rewardUnit.UnitLabel,
            rewardUnit.TryMultiplier);
    }

    public async Task AddManualTransactionAsync(int subjectId, decimal amount, string description, int adminId)
    {
        if (amount == 0)
            throw new BusinessException("INVALID_AMOUNT", "Tutar sıfır olamaz.");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException("DESCRIPTION_REQUIRED", "Açıklama zorunludur.");

        var wallet = await walletRepository.GetBySubjectIdAsync(subjectId)
            ?? throw new NotFoundException("Cüzdan");

        var absAmount = Math.Abs(amount);
        var isCredit = amount > 0;

        if (!isCredit && wallet.Balance < absAmount)
            throw new BusinessException("INSUFFICIENT_BALANCE", "Eksi hareket için bakiye yetersiz.");

        wallet.Balance = isCredit
            ? wallet.Balance + absAmount
            : wallet.Balance - absAmount;
        wallet.SetUpdated(adminId);
        await walletRepository.UpdateAsync(wallet);

        var tx = new WalletTransaction
        {
            WalletId = wallet.Id,
            Amount = absAmount,
            Type = isCredit ? WalletTransactionType.Credit : WalletTransactionType.Withdrawal,
            ReferenceId = $"manual:{Guid.NewGuid():N}",
            Description = description.Trim()
        };
        tx.SetCreated(adminId);
        await walletRepository.AddTransactionAsync(tx);

        logger.LogInformation(
            "Manuel cüzdan hareketi eklendi: SubjectId={SubjectId} Amount={Amount} Type={Type} AdminId={AdminId}",
            subjectId, absAmount, tx.Type, adminId);
    }

    public async Task DeleteManualTransactionAsync(int subjectId, int transactionId, int adminId)
    {
        var wallet = await walletRepository.GetBySubjectIdAsync(subjectId)
            ?? throw new NotFoundException("Cüzdan");

        var transaction = await walletRepository.GetTransactionByIdAsync(wallet.Id, transactionId)
            ?? throw new NotFoundException("Hesap hareketi");

        if (!IsManualReference(transaction.ReferenceId))
            throw new BusinessException("NOT_MANUAL_TRANSACTION", "Sadece manuel hareketler silinebilir.");

        if (transaction.Type == WalletTransactionType.Credit)
        {
            if (wallet.Balance < transaction.Amount)
                throw new BusinessException(
                    "INSUFFICIENT_BALANCE",
                    "Bu artı hareket silinemez. Güncel bakiye yetersiz.");

            wallet.Balance -= transaction.Amount;
        }
        else
        {
            wallet.Balance += transaction.Amount;
        }

        wallet.SetUpdated(adminId);
        await walletRepository.UpdateAsync(wallet);
        await walletRepository.DeleteTransactionAsync(transaction);

        logger.LogInformation(
            "Manuel cüzdan hareketi silindi: SubjectId={SubjectId} TransactionId={TransactionId} AdminId={AdminId}",
            subjectId, transactionId, adminId);
    }

    public async Task CreditAsync(int subjectId, decimal amount, string referenceId, string description)
    {
        var wallet = await walletRepository.GetBySubjectIdAsync(subjectId)
            ?? throw new NotFoundException("Cüzdan");

        wallet.Balance    += amount;
        wallet.TotalEarned += amount;
        wallet.SetUpdated(subjectId);

        await walletRepository.UpdateAsync(wallet);

        var transaction = new WalletTransaction
        {
            WalletId    = wallet.Id,
            Amount      = amount,
            Type        = WalletTransactionType.Credit,
            ReferenceId = referenceId,
            Description = description
        };
        transaction.SetCreated(subjectId);
        await walletRepository.AddTransactionAsync(transaction);

        logger.LogInformation("Cüzdan kredilendirildi: SubjectId={SubjectId} Amount={Amount} Ref={ReferenceId}",
            subjectId, amount, referenceId);

        var creditTitle = "Kazanç!";
        var rewardUnit = await rewardUnitConfigService.GetAsync();
        var creditBody = $"{amount:F2} {rewardUnit.UnitLabel} hesabınıza eklendi.";
        const string creditType = "wallet_credit";

        var creditNotification = new Notification
        {
            SubjectId = subjectId,
            Title = creditTitle,
            Body = creditBody,
            Type = creditType
        };
        creditNotification.SetCreated(0);
        await notificationRepository.AddAsync(creditNotification);

        var subject = await subjectRepository.GetByIdAsync(subjectId);
        if (subject?.FcmToken is not null)
        {
            await publisher.PublishAsync(
                new NotificationSendMessage(
                    creditNotification.Id,
                    subjectId,
                    subject.FcmToken,
                    creditTitle,
                    creditBody,
                    creditType),
                Queues.NotificationSend);
        }
        else
        {
            await notificationRepository.UpdateDeliveryStatusAsync(
                creditNotification.Id,
                DeliveryStatus.Skipped,
                null);
        }
    }

    public async Task RequestWithdrawalAsync(int subjectId, WithdrawalRequestDto dto)
    {
        await withdrawalValidator.ValidateAndThrowAsync(dto);

        var rewardUnit = await rewardUnitConfigService.GetAsync();
        var amountTry = decimal.Round(dto.Amount * rewardUnit.TryMultiplier, 2, MidpointRounding.AwayFromZero);
        var minWithdrawalTry = await GetMinWithdrawalThresholdTryAsync();
        if (minWithdrawalTry > 0m && amountTry < minWithdrawalTry)
        {
            var minAmountInUnit = rewardUnit.TryMultiplier > 0m
                ? decimal.Round(minWithdrawalTry / rewardUnit.TryMultiplier, 2, MidpointRounding.AwayFromZero)
                : minWithdrawalTry;

            throw new BusinessException(
                "WITHDRAWAL_MIN_THRESHOLD",
                $"Minimum çekim tutarı {minAmountInUnit:F2} {rewardUnit.UnitLabel} ({minWithdrawalTry:F2} TL) olmalıdır.");
        }

        var bankAccount = await walletRepository.GetBankAccountAsync(subjectId, dto.BankAccountId)
            ?? throw new NotFoundException("Banka hesabı");
        var referenceId = $"withdrawal:{Guid.NewGuid():N}";
        var requestedAt = DateTime.UtcNow;
        await walletRepository.CreateWithdrawalTransactionAsync(
            subjectId,
            dto.BankAccountId,
            dto.Amount,
            amountTry,
            rewardUnit.UnitCode,
            rewardUnit.UnitLabel,
            rewardUnit.TryMultiplier,
            requestedAt,
            referenceId,
            $"Para çekimi — {bankAccount.BankName} ...{bankAccount.IbanLast4}");

        logger.LogInformation("Para çekimi talep edildi: SubjectId={SubjectId} Amount={Amount}", subjectId, dto.Amount);
    }

    public async Task ApproveWithdrawalAsync(int requestId, int adminId)
    {
        var request = await withdrawalRequestRepository.GetByIdAsync(requestId)
            ?? throw new NotFoundException("Para çekimi talebi");

        if (request.Status != ApprovalStatus.Pending)
            throw new BusinessException("ALREADY_PROCESSED", "Bu talep zaten işlenmiş.");

        request.Status      = ApprovalStatus.Approved;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = adminId;
        request.SetUpdated(adminId);
        await withdrawalRequestRepository.UpdateAsync(request);

        logger.LogInformation("Para çekimi onaylandı: RequestId={RequestId} SubjectId={SubjectId} Amount={Amount}",
            requestId, request.SubjectId, request.Amount);
    }

    public async Task RejectWithdrawalAsync(int requestId, string reason, int adminId)
    {
        var request = await withdrawalRequestRepository.GetByIdAsync(requestId)
            ?? throw new NotFoundException("Para çekimi talebi");

        if (request.Status != ApprovalStatus.Pending)
            throw new BusinessException("ALREADY_PROCESSED", "Bu talep zaten işlenmiş.");

        request.Status          = ApprovalStatus.Rejected;
        request.RejectionReason = reason;
        request.ProcessedAt     = DateTime.UtcNow;
        request.ProcessedBy     = adminId;
        request.SetUpdated(adminId);

        var wallet = await walletRepository.GetBySubjectIdAsync(request.SubjectId)
            ?? throw new NotFoundException("Cüzdan");

        wallet.Balance += request.Amount;
        wallet.SetUpdated(0);
        await walletRepository.UpdateAsync(wallet);

        var refundTransaction = new WalletTransaction
        {
            WalletId    = wallet.Id,
            Amount      = request.Amount,
            Type        = WalletTransactionType.Credit,
            ReferenceId = $"refund:{request.WalletTransactionId}",
            Description = "Para çekimi iadesi"
        };
        refundTransaction.SetCreated(0);
        await walletRepository.AddTransactionAsync(refundTransaction);

        await withdrawalRequestRepository.UpdateAsync(request);

        var rejectedTitle = "Para Çekimi Reddedildi";
        var rewardUnit = await rewardUnitConfigService.GetAsync();
        var rejectedBody = $"{request.Amount:F2} {rewardUnit.UnitLabel} tutarındaki çekim talebiniz reddedildi. Neden: {reason}";
        const string rejectedType = "withdrawal_rejected";

        var rejectedNotification = new Notification
        {
            SubjectId = request.SubjectId,
            Title = rejectedTitle,
            Body = rejectedBody,
            Type = rejectedType
        };
        rejectedNotification.SetCreated(adminId);
        await notificationRepository.AddAsync(rejectedNotification);

        var subject = await subjectRepository.GetByIdAsync(request.SubjectId);
        if (subject?.FcmToken is not null)
        {
            await publisher.PublishAsync(
                new NotificationSendMessage(
                    rejectedNotification.Id,
                    request.SubjectId,
                    subject.FcmToken,
                    rejectedTitle,
                    rejectedBody,
                    rejectedType),
                Queues.NotificationSend);
        }
        else
        {
            await notificationRepository.UpdateDeliveryStatusAsync(
                rejectedNotification.Id,
                DeliveryStatus.Skipped,
                null);
        }

        logger.LogInformation("Para çekimi reddedildi: RequestId={RequestId} SubjectId={SubjectId} Amount={Amount} Reason={Reason}",
            requestId, request.SubjectId, request.Amount, reason);
    }

    public async Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(int subjectId, int page = 1, int pageSize = 20)
    {
        var wallet = await walletRepository.GetBySubjectIdAsync(subjectId)
            ?? throw new NotFoundException("Cüzdan");
        var rewardUnit = await rewardUnitConfigService.GetAsync();

        var total        = await walletRepository.CountTransactionsAsync(wallet.Id);
        var transactions = await walletRepository.GetTransactionsAsync(
            wallet.Id,
            skip: (page - 1) * pageSize,
            take: pageSize);

        var items = transactions
            .Select(t => new WalletTransactionDto(
                t.Id,
                t.Amount,
                t.Type,
                NormalizeDescription(t.Description),
                t.CreatedAt,
                IsManualReference(t.ReferenceId),
                rewardUnit.UnitLabel))
            .ToList();

        return new PagedResult<WalletTransactionDto>(items, total, page, pageSize);
    }

    public async Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(int subjectId)
    {
        var accounts = await walletRepository.GetBankAccountsAsync(subjectId);
        return accounts.Select(b => new BankAccountDto(b.Id, b.BankName, b.IbanLast4, b.IsDefault));
    }

    public async Task AddBankAccountAsync(int subjectId, AddBankAccountDto dto)
    {
        await bankAccountValidator.ValidateAndThrowAsync(dto);

        var existing = await walletRepository.GetBankAccountsAsync(subjectId);

        var account = new BankAccount
        {
            SubjectId      = subjectId,
            BankName       = dto.BankName,
            IbanLast4      = dto.Iban[^4..],
            IbanEncrypted  = dto.Iban,
            IsDefault      = existing.Count == 0
        };
        account.SetCreated(subjectId);

        await walletRepository.AddBankAccountAsync(account);

        logger.LogInformation("Banka hesabı eklendi: SubjectId={SubjectId}", subjectId);
    }

    public async Task DeleteBankAccountAsync(int subjectId, int accountId)
    {
        var account = await walletRepository.GetBankAccountAsync(subjectId, accountId)
            ?? throw new NotFoundException("Banka hesabı");

        await walletRepository.DeleteBankAccountAsync(account);

        logger.LogInformation("Banka hesabı silindi: SubjectId={SubjectId} AccountId={AccountId}", subjectId, accountId);
    }

    public async Task<List<WalletLedgerDto>> GetLedgerAsync(int subjectId, int page = 1, int pageSize = 50)
    {
        var wallet = await walletRepository.GetBySubjectIdAsync(subjectId)
            ?? throw new NotFoundException("Cüzdan");
        var rewardUnit = await rewardUnitConfigService.GetAsync();

        var skip = (page - 1) * pageSize;
        var transactions = await walletRepository.GetTransactionsAsync(wallet.Id, skip, pageSize);

        // Calculate cumulative balance from oldest to newest
        var allTransactions = await walletRepository.GetTransactionsAsync(wallet.Id, 0, int.MaxValue);
        var orderedAll = allTransactions.OrderBy(t => t.CreatedAt).ToList();

        decimal runningBalance = wallet.Balance;
        var balanceMap = new Dictionary<int, decimal>();

        foreach (var txn in orderedAll.AsEnumerable().Reverse())
        {
            balanceMap[txn.Id] = runningBalance;
            var amount = txn.Type == WalletTransactionType.Credit ? txn.Amount : -txn.Amount;
            runningBalance -= amount;
        }

        // Return page results with cumulative balance
        return transactions
            .Select(t => new WalletLedgerDto(
                t.Id,
                t.Amount,
                t.Type,
                NormalizeDescription(t.Description),
                t.CreatedAt,
                balanceMap.GetValueOrDefault(t.Id, wallet.Balance),
                IsManualReference(t.ReferenceId),
                rewardUnit.UnitLabel
            ))
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    private static bool IsManualReference(string? referenceId)
        => !string.IsNullOrWhiteSpace(referenceId) &&
           referenceId.StartsWith("manual:", StringComparison.OrdinalIgnoreCase);

    private static bool IsRewardEligibleStatus(AssignmentStatus status)
        => status is AssignmentStatus.Completed
            or AssignmentStatus.Disqualify
            or AssignmentStatus.QuotaFull
            or AssignmentStatus.ScreenOut;

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return description;

        const string manualPrefix = "Manuel hareket:";
        return description.StartsWith(manualPrefix, StringComparison.OrdinalIgnoreCase)
            ? description[manualPrefix.Length..].Trim()
            : description;
    }

    private async Task<decimal> GetMinWithdrawalThresholdTryAsync()
    {
        var setting = await paymentSettingRepository.GetByKeyAsync(PaymentSettingKeys.WithdrawalMinAmountTry);
        if (setting is null || string.IsNullOrWhiteSpace(setting.Value))
            return 0m;

        return TryParseDecimal(setting.Value, out var parsed) && parsed > 0m
            ? parsed
            : 0m;
    }

    private static bool TryParseDecimal(string rawValue, out decimal value)
    {
        var normalized = rawValue.Trim();
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return true;

        if (decimal.TryParse(normalized, NumberStyles.Number, new CultureInfo("tr-TR"), out value))
            return true;

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
    }
}
