using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class PaymentBatchRepository(AppDbContext db) : IPaymentBatchRepository
{
    public Task<PaymentBatch?> GetByIdAsync(int id)
        => db.PaymentBatches.FirstOrDefaultAsync(b => b.Id == id);

    public Task<PaymentBatch?> GetDetailAsync(int id)
        => db.PaymentBatches
             .Include(b => b.Items)
                 .ThenInclude(i => i.WithdrawalRequest)
                     .ThenInclude(r => r.Subject)
             .Include(b => b.Items)
                 .ThenInclude(i => i.WithdrawalRequest)
                     .ThenInclude(r => r.BankAccount)
             .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<(List<PaymentBatch> Items, int Total)> GetPagedAsync(PaymentBatchStatus? status, int skip, int take)
    {
        var query = db.PaymentBatches.AsQueryable();
        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return (items, total);
    }

    public async Task<int> GetNextSequenceAsync(string datePrefix)
    {
        var prefix = $"PB-{datePrefix}-";
        var count  = await db.PaymentBatches
            .CountAsync(b => b.BatchNumber.StartsWith(prefix));
        return count + 1;
    }

    public async Task AddAsync(PaymentBatch batch)
    {
        db.PaymentBatches.Add(batch);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PaymentBatch batch)
    {
        db.PaymentBatches.Update(batch);
        await db.SaveChangesAsync();
    }
}
