using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class WithdrawalRequestRepository(AppDbContext db) : IWithdrawalRequestRepository
{
    public async Task AddAsync(WithdrawalRequest request)
    {
        db.WithdrawalRequests.Add(request);
        await db.SaveChangesAsync();
    }

    public Task<WithdrawalRequest?> GetByIdAsync(int id)
        => db.WithdrawalRequests
             .Include(w => w.Subject)
             .Include(w => w.BankAccount)
             .FirstOrDefaultAsync(w => w.Id == id);

    public Task<WithdrawalRequest?> GetByTransactionIdAsync(int walletTransactionId)
        => db.WithdrawalRequests
             .FirstOrDefaultAsync(w => w.WalletTransactionId == walletTransactionId);

    public async Task<(List<WithdrawalRequest> Items, int Total)> GetPagedAsync(ApprovalStatus status, int skip, int take)
    {
        var query = db.WithdrawalRequests
            .Include(w => w.Subject)
            .Include(w => w.BankAccount)
            .Where(w => w.Status == status);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        return (items, total);
    }

    public Task<List<WithdrawalRequest>> GetApprovedWithoutBatchAsync()
        => db.WithdrawalRequests
             .Include(w => w.Subject)
             .Include(w => w.BankAccount)
             .Where(w => w.Status == ApprovalStatus.Approved &&
                         !db.PaymentBatchItems.Any(i => i.WithdrawalRequestId == w.Id))
             .OrderBy(w => w.CreatedAt)
             .ToListAsync();

    public async Task UpdateAsync(WithdrawalRequest request)
    {
        db.WithdrawalRequests.Update(request);
        await db.SaveChangesAsync();
    }
}
