using Microsoft.Extensions.Logging;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class ExternalAffiliateService(
    IExternalAffiliateRepository affiliateRepository,
    ISubjectRepository subjectRepository,
    ILogger<ExternalAffiliateService> logger) : IExternalAffiliateService
{
    public Task<List<ExternalAffiliate>> GetAllAsync()
        => affiliateRepository.GetAllOrderedAsync();

    public Task<ExternalAffiliate?> GetByIdAsync(int id)
        => affiliateRepository.GetByIdAsync(id);

    public Task<ExternalAffiliate?> GetByIdWithTransactionsAsync(int id)
        => affiliateRepository.GetByIdWithTransactionsAsync(id);

    public async Task CreateOrUpdateAsync(int id, string name, string email, string? phone, string? iban,
        string affiliateCode, decimal? commissionAmount,
        bool isActive, string? note, int adminId)
    {
        var normalizedCode = affiliateCode.Trim().ToUpperInvariant();
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await affiliateRepository.ExistsByCodeAsync(normalizedCode, id))
            throw new BusinessException("DUPLICATE_CODE", "Bu affiliate kodu zaten kullanılıyor.");

        if (await affiliateRepository.ExistsByEmailAsync(normalizedEmail, id))
            throw new BusinessException("DUPLICATE_EMAIL", "Bu e-posta adresi zaten kullanılıyor.");

        // Denek referral kodu ile çakışma kontrolü
        var subjectWithCode = await subjectRepository.GetByReferralCodeAsync(normalizedCode);
        if (subjectWithCode is not null)
            throw new BusinessException("CODE_CONFLICT", "Bu kod bir denek referral kodu olarak kullanılıyor.");

        ExternalAffiliate affiliate;
        if (id == 0)
        {
            affiliate = new ExternalAffiliate();
            affiliate.SetCreated(adminId);
        }
        else
        {
            affiliate = await affiliateRepository.GetByIdAsync(id)
                        ?? throw new NotFoundException("Affiliate");
            affiliate.SetUpdated(adminId);
        }

        affiliate.Name = name.Trim();
        affiliate.Email = normalizedEmail;
        affiliate.Phone = phone?.Trim();
        affiliate.Iban = iban?.Trim().ToUpperInvariant();
        affiliate.AffiliateCode = normalizedCode;
        affiliate.CommissionAmount = commissionAmount;
        affiliate.IsActive = isActive;
        affiliate.Note = note?.Trim();

        if (id == 0)
            await affiliateRepository.AddAsync(affiliate);
        else
            await affiliateRepository.UpdateAsync(affiliate);

        logger.LogInformation(
            "Affiliate {Action}: Id={AffiliateId} Code={Code} by Admin {AdminId}",
            id == 0 ? "oluşturuldu" : "güncellendi", affiliate.Id, normalizedCode, adminId);
    }

    public async Task RecordPaymentAsync(int affiliateId, decimal amount, string? description, int adminId)
    {
        if (amount <= 0)
            throw new BusinessException("INVALID_AMOUNT", "Ödeme tutarı sıfırdan büyük olmalıdır.");

        var affiliate = await affiliateRepository.GetByIdAsync(affiliateId)
                        ?? throw new NotFoundException("Affiliate");

        if (affiliate.Balance < amount)
            throw new BusinessException("INSUFFICIENT_BALANCE", "Yetersiz bakiye.");

        affiliate.Balance -= amount;
        affiliate.TotalPaid += amount;
        affiliate.SetUpdated(adminId);

        var tx = new AffiliateTransaction
        {
            ExternalAffiliateId = affiliateId,
            Type = AffiliateTransactionType.Payment,
            Amount = amount,
            Description = description?.Trim()
        };
        tx.SetCreated(adminId);

        // Bakiye + hareket tek transaction içinde, concurrency korumalı
        await affiliateRepository.RecordPaymentTransactionAsync(affiliate, tx);

        logger.LogInformation(
            "Affiliate ödeme kaydedildi: AffiliateId={AffiliateId} Amount={Amount} by Admin {AdminId}",
            affiliateId, amount, adminId);
    }

    public async Task RecordMovementAsync(int affiliateId, decimal amount, bool isCredit, string? description, int adminId)
    {
        if (amount <= 0)
            throw new BusinessException("INVALID_AMOUNT", "Tutar sıfırdan büyük olmalıdır.");

        var affiliate = await affiliateRepository.GetByIdAsync(affiliateId)
                        ?? throw new NotFoundException("Affiliate");

        if (!isCredit && affiliate.Balance < amount)
            throw new BusinessException("INSUFFICIENT_BALANCE", "Eksi hareket için bakiye yetersiz.");

        if (isCredit)
        {
            affiliate.Balance += amount;
            affiliate.TotalEarned += amount;
        }
        else
        {
            affiliate.Balance -= amount;
        }

        affiliate.SetUpdated(adminId);

        var tx = new AffiliateTransaction
        {
            ExternalAffiliateId = affiliateId,
            Type = isCredit ? AffiliateTransactionType.Credit : AffiliateTransactionType.Debit,
            Amount = amount,
            Description = description?.Trim()
        };
        tx.SetCreated(adminId);

        await affiliateRepository.RecordAdjustmentTransactionAsync(affiliate, tx);

        logger.LogInformation(
            "Affiliate hareket kaydedildi: AffiliateId={AffiliateId} Type={Type} Amount={Amount} by Admin {AdminId}",
            affiliateId, tx.Type, amount, adminId);
    }

    public async Task DeleteTransactionAsync(int affiliateId, int transactionId, int adminId)
    {
        var affiliate = await affiliateRepository.GetByIdAsync(affiliateId)
                        ?? throw new NotFoundException("Affiliate");

        var tx = await affiliateRepository.GetTransactionByIdAsync(transactionId)
                 ?? throw new NotFoundException("İşlem");

        if (tx.ExternalAffiliateId != affiliateId)
            throw new BusinessException("INVALID_TRANSACTION", "İşlem bu ortağa ait değil.");

        if (tx.Type == AffiliateTransactionType.Commission)
            throw new BusinessException("CANNOT_DELETE_COMMISSION", "Komisyon işlemleri silinemez.");

        // Bakiyeyi geri al
        switch (tx.Type)
        {
            case AffiliateTransactionType.Payment:
                affiliate.Balance += tx.Amount;
                affiliate.TotalPaid -= tx.Amount;
                break;
            case AffiliateTransactionType.Credit:
                if (affiliate.Balance < tx.Amount)
                    throw new BusinessException("INSUFFICIENT_BALANCE",
                        "Bu artı hareket silinemez. Güncel bakiye yetersiz.");
                affiliate.Balance -= tx.Amount;
                affiliate.TotalEarned -= tx.Amount;
                break;
            case AffiliateTransactionType.Debit:
                affiliate.Balance += tx.Amount;
                break;
        }

        affiliate.SetUpdated(adminId);
        await affiliateRepository.DeleteTransactionAsync(affiliate, tx);

        logger.LogInformation(
            "Affiliate işlem silindi: AffiliateId={AffiliateId} TransactionId={TransactionId} Type={Type} Amount={Amount} by Admin {AdminId}",
            affiliateId, transactionId, tx.Type, tx.Amount, adminId);
    }

    public Task<List<Subject>> GetReferredSubjectsAsync(int affiliateId)
        => affiliateRepository.GetReferredSubjectsAsync(affiliateId);
}
