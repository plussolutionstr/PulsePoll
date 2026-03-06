using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class BankRepository(AppDbContext db) : IBankRepository
{
    public Task<List<Bank>> GetAllOrderedAsync()
        => db.Banks.OrderBy(b => b.Name).ToListAsync();

    public Task<Bank?> GetByIdAsync(int id)
        => db.Banks.FirstOrDefaultAsync(b => b.Id == id);

    public Task<bool> ExistsByNameAsync(string name, int excludeId)
        => db.Banks.AnyAsync(b => b.Name == name && b.Id != excludeId);

    public async Task AddAsync(Bank bank)
    {
        db.Banks.Add(bank);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Bank bank)
    {
        db.Banks.Update(bank);
        await db.SaveChangesAsync();
    }
}
