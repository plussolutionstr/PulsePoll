using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SurveyResultScriptRepository(AppDbContext db) : ISurveyResultScriptRepository
{
    public Task<List<SurveyResultScript>> GetAllAsync(bool includeInactive = true)
    {
        var query = db.SurveyResultScripts
            .Include(x => x.Patterns.Where(p => p.DeletedAt == null))
            .Where(x => x.DeletedAt == null);

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        return query
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<SurveyResultScript?> GetByIdAsync(int id)
        => db.SurveyResultScripts
            .Include(x => x.Patterns.Where(p => p.DeletedAt == null))
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLower();

        var query = db.SurveyResultScripts
            .Where(x => x.DeletedAt == null && x.Name.ToLower() == normalized);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return query.AnyAsync();
    }

    public async Task AddAsync(SurveyResultScript script)
    {
        db.SurveyResultScripts.Add(script);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(SurveyResultScript script)
    {
        db.SurveyResultScripts.Update(script);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(SurveyResultScript script)
    {
        db.SurveyResultScripts.Update(script);
        await db.SaveChangesAsync();
    }
}
