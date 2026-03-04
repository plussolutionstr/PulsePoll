using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class TaxOfficeRepository(AppDbContext db) : ITaxOfficeRepository
{
    public Task<List<TaxOffice>> GetAllAsync()
        => db.TaxOffices.OrderBy(t => t.Name).ToListAsync();

    public Task<List<TaxOffice>> GetByCityIdAsync(int cityId)
        => db.TaxOffices.Where(t => t.CityId == cityId).OrderBy(t => t.Name).ToListAsync();

    public Task<TaxOffice?> GetByIdAsync(int id)
        => db.TaxOffices.FindAsync(id).AsTask();
}
