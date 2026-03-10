using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IExternalAffiliateService
{
    Task<List<ExternalAffiliate>> GetAllAsync();
    Task<ExternalAffiliate?> GetByIdAsync(int id);
    Task<ExternalAffiliate?> GetByIdWithTransactionsAsync(int id);
    Task CreateOrUpdateAsync(int id, string name, string email, string? phone, string? iban,
        string affiliateCode, decimal? commissionAmount,
        bool isActive, string? note, int adminId);
    Task RecordPaymentAsync(int affiliateId, decimal amount, string? description, int adminId);
    Task RecordMovementAsync(int affiliateId, decimal amount, bool isCredit, string? description, int adminId);
    Task DeleteTransactionAsync(int affiliateId, int transactionId, int adminId);
    Task<List<Subject>> GetReferredSubjectsAsync(int affiliateId);
}
