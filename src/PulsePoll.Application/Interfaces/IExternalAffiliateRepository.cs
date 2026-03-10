using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IExternalAffiliateRepository
{
    Task<List<ExternalAffiliate>> GetAllOrderedAsync();
    Task<ExternalAffiliate?> GetByIdAsync(int id);
    Task<ExternalAffiliate?> GetByIdWithTransactionsAsync(int id);
    Task<ExternalAffiliate?> GetByAffiliateCodeAsync(string code);
    Task<bool> ExistsByCodeAsync(string code, int excludeId = 0);
    Task<bool> ExistsByEmailAsync(string email, int excludeId = 0);
    Task AddAsync(ExternalAffiliate affiliate);
    Task UpdateAsync(ExternalAffiliate affiliate);
    Task<AffiliateTransaction?> GetTransactionByReferenceAsync(int affiliateId, string referenceId);
    Task<List<Subject>> GetReferredSubjectsAsync(int affiliateId);
    Task<List<int>> GetPendingCommissionSubjectIdsAsync();

    /// <summary>
    /// Affiliate bakiyesini günceller ve hareket kaydı oluşturur — tek transaction içinde.
    /// Concurrency kontrolü yapar (DbUpdateConcurrencyException → BusinessException).
    /// </summary>
    /// <returns>true = komisyon yazıldı, false = idempotent no-op (unique constraint çakışması)</returns>
    Task<bool> GrantCommissionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction);

    /// <summary>Ödeme: bakiye düşer, TotalPaid artar + hareket — tek transaction.</summary>
    Task RecordPaymentTransactionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction);

    /// <summary>Manuel hareket (Credit/Debit): bakiye ± değişir + hareket — tek transaction.</summary>
    Task RecordAdjustmentTransactionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction);

    /// <summary>Belirtilen ID'ye sahip hareketi getirir.</summary>
    Task<AffiliateTransaction?> GetTransactionByIdAsync(int transactionId);

    /// <summary>Manuel hareketi siler ve bakiyeyi geri alır — tek transaction.</summary>
    Task DeleteTransactionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction);

    /// <summary>
    /// Reconciliation için: ExternalAffiliateId != null ve henüz komisyon almamış
    /// subject'leri affiliate bilgileriyle birlikte toplu yükler.
    /// </summary>
    Task<List<Subject>> GetPendingCommissionSubjectsWithAffiliateAsync();
}
