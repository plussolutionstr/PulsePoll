using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SmsLogRepository(AppDbContext db) : ISmsLogRepository
{
    public async Task AddAsync(SmsLog log)
    {
        db.SmsLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<(List<SmsLog> Items, int Total)> GetPagedAsync(
        int skip,
        int take,
        string? phoneFilter = null,
        SmsSource? sourceFilter = null,
        DeliveryStatus? statusFilter = null)
    {
        var query = db.SmsLogs
            .Include(s => s.Subject)
            .Where(s => s.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(phoneFilter))
            query = query.Where(s => s.PhoneNumber.Contains(phoneFilter));

        if (sourceFilter.HasValue)
            query = query.Where(s => s.Source == sourceFilter.Value);

        if (statusFilter.HasValue)
            query = query.Where(s => s.DeliveryStatus == statusFilter.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, total);
    }
}
