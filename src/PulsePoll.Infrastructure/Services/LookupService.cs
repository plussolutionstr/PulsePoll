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

    public Task<List<Bank>> GetBanksAsync(bool onlyActive = true)
    {
        var query = db.Banks.AsQueryable();
        if (onlyActive)
            query = query.Where(b => b.IsActive);

        return query
            .Include(b => b.ThumbnailMediaAsset)
            .Include(b => b.LogoMediaAsset)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public Task<Bank?> GetBankByIdAsync(int bankId)
        => db.Banks.FirstOrDefaultAsync(b => b.Id == bankId);

    public Task<List<Profession>> GetProfessionsAsync()
        => db.Professions.OrderBy(p => p.Id).ToListAsync();

    public Task<List<EducationLevel>> GetEducationLevelsAsync()
        => db.EducationLevels.OrderBy(e => e.OrderIndex).ToListAsync();
}
