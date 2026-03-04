using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(int id)
        => db.Customers
             .Include(c => c.TaxOffice)
             .Include(c => c.City)
             .Include(c => c.District)
             .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);

    public Task<Customer?> GetByCodeAsync(string code)
        => db.Customers.FirstOrDefaultAsync(c => c.Code == code && c.DeletedAt == null);

    public Task<List<Customer>> GetAllAsync()
        => db.Customers
             .Include(c => c.TaxOffice)
             .Include(c => c.City)
             .Include(c => c.District)
             .Where(c => c.DeletedAt == null)
             .OrderBy(c => c.ShortName)
             .ToListAsync();

    public Task<List<Customer>> GetPagedAsync(int skip, int take)
        => db.Customers
             .Include(c => c.TaxOffice)
             .Include(c => c.City)
             .Include(c => c.District)
             .Where(c => c.DeletedAt == null)
             .OrderBy(c => c.ShortName)
             .Skip(skip)
             .Take(take)
             .ToListAsync();

    public Task<int> CountAsync()
        => db.Customers.CountAsync(c => c.DeletedAt == null);

    public Task<bool> ExistsByCodeAsync(string code)
        => db.Customers.AnyAsync(c => c.Code == code && c.DeletedAt == null);

    public Task<bool> ExistsByTaxNumberAsync(string taxNumber)
        => db.Customers.AnyAsync(c => c.TaxNumber == taxNumber && c.DeletedAt == null);

    public async Task AddAsync(Customer customer)
    {
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        db.Customers.Update(customer);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        db.Customers.Update(customer);
        await db.SaveChangesAsync();
    }
}
