using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ITaxOfficeRepository
{
    Task<List<TaxOffice>> GetAllAsync();
    Task<List<TaxOffice>> GetByCityIdAsync(int cityId);
    Task<TaxOffice?> GetByIdAsync(int id);
}
