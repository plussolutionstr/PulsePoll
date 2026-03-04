using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IPaymentSettingRepository
{
    Task<List<PaymentSetting>> GetAllAsync();
    Task<PaymentSetting?> GetByKeyAsync(string key);
    Task UpsertAsync(string key, string value, int adminId, string? description = null);
}
