using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SpecialDayRepository(AppDbContext db) : ISpecialDayRepository
{
    public Task<List<SpecialDay>> GetByYearAsync(int year)
        => db.SpecialDays
            .AsNoTracking()
            .Where(x => x.Date.Year == year)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Name)
            .ToListAsync();

    public Task<List<SpecialDay>> GetByDateAsync(DateOnly date)
        => db.SpecialDays
            .AsNoTracking()
            .Where(x => x.Date == date)
            .ToListAsync();

    public Task<SpecialDay?> GetByIdAsync(int id)
        => db.SpecialDays.FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> ExistsByEventCodeAndDateAsync(string eventCode, DateOnly date, int? excludeId = null)
        => db.SpecialDays.AnyAsync(x =>
            x.EventCode == eventCode &&
            x.Date == date &&
            (!excludeId.HasValue || x.Id != excludeId.Value));

    public async Task ReplaceSystemYearAsync(int year, IEnumerable<SpecialDay> systemDays)
    {
        var existing = await db.SpecialDays
            .Where(x => x.Date.Year == year && x.Source == "system")
            .ToListAsync();

        if (existing.Count > 0)
            db.SpecialDays.RemoveRange(existing);

        db.SpecialDays.AddRange(systemDays);
        await db.SaveChangesAsync();
    }

    public async Task AddAsync(SpecialDay day)
    {
        db.SpecialDays.Add(day);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(SpecialDay day)
    {
        db.SpecialDays.Update(day);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(SpecialDay day)
    {
        db.SpecialDays.Remove(day);
        await db.SaveChangesAsync();
    }
}
