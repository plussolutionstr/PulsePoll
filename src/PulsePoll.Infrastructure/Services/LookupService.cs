using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Infrastructure.Persistence;

namespace PulsePoll.Infrastructure.Services;

public class LookupService(AppDbContext db) : ILookupService
{
    public Task<List<City>> GetCitiesAsync()
        => db.Cities.OrderBy(c => c.Name).ToListAsync();

    public Task<List<District>> GetDistrictsByCityIdAsync(int cityId)
        => db.Districts.Where(d => d.CityId == cityId).OrderBy(d => d.Name).ToListAsync();

    public Task<List<TaxOffice>> GetTaxOfficesByCityIdAsync(int cityId)
        => db.TaxOffices.Where(t => t.CityId == cityId).OrderBy(t => t.Name).ToListAsync();
}
