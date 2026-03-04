using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ILookupService
{
    Task<List<City>> GetCitiesAsync();
    Task<List<District>> GetDistrictsByCityIdAsync(int cityId);
    Task<List<TaxOffice>> GetTaxOfficesByCityIdAsync(int cityId);
}
