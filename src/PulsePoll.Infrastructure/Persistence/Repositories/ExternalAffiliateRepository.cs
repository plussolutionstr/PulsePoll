using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class ExternalAffiliateRepository(AppDbContext db) : IExternalAffiliateRepository
{
    public async Task<List<ExternalAffiliate>> GetAllOrderedAsync()
        => await db.ExternalAffiliates
            .Where(a => a.DeletedAt == null)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<ExternalAffiliate?> GetByIdAsync(int id)
        => await db.ExternalAffiliates
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);

    public async Task<ExternalAffiliate?> GetByIdWithTransactionsAsync(int id)
        => await db.ExternalAffiliates
            .Include(a => a.Transactions.OrderByDescending(t => t.CreatedAt))
                .ThenInclude(t => t.Subject)
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);

    public async Task<ExternalAffiliate?> GetByAffiliateCodeAsync(string code)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await db.ExternalAffiliates
            .FirstOrDefaultAsync(a => a.AffiliateCode == normalized && a.DeletedAt == null);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int excludeId = 0)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await db.ExternalAffiliates
            .AnyAsync(a => a.AffiliateCode == normalized && a.Id != excludeId && a.DeletedAt == null);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int excludeId = 0)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await db.ExternalAffiliates
            .AnyAsync(a => a.Email == normalized && a.Id != excludeId && a.DeletedAt == null);
    }

    public async Task AddAsync(ExternalAffiliate affiliate)
    {
        db.ExternalAffiliates.Add(affiliate);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ExternalAffiliate affiliate)
    {
        db.ExternalAffiliates.Update(affiliate);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException("CONCURRENT_REQUEST",
                "Eşzamanlı işlem tespit edildi. Lütfen tekrar deneyin.");
        }
    }

    public async Task<AffiliateTransaction?> GetTransactionByReferenceAsync(int affiliateId, string referenceId)
        => await db.AffiliateTransactions
            .FirstOrDefaultAsync(t => t.ExternalAffiliateId == affiliateId && t.ReferenceId == referenceId);

    public async Task<List<Subject>> GetReferredSubjectsAsync(int affiliateId)
        => await db.Subjects
            .Include(s => s.City)
            .Where(s => s.ExternalAffiliateId == affiliateId && s.DeletedAt == null)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<List<int>> GetPendingCommissionSubjectIdsAsync()
        => await db.Subjects
            .Where(s => s.ExternalAffiliateId != null
                        && s.DeletedAt == null
                        && !db.AffiliateTransactions.Any(t =>
                            t.ExternalAffiliateId == s.ExternalAffiliateId
                            && t.SubjectId == s.Id
                            && t.Type == Domain.Enums.AffiliateTransactionType.Commission))
            .Select(s => s.Id)
            .ToListAsync();

    public async Task<List<Subject>> GetPendingCommissionSubjectsWithAffiliateAsync()
        => await db.Subjects
            .Include(s => s.ExternalAffiliate)
            .Where(s => s.ExternalAffiliateId != null
                        && s.DeletedAt == null
                        && s.ExternalAffiliate!.IsActive
                        && s.ExternalAffiliate.DeletedAt == null
                        && !db.AffiliateTransactions.Any(t =>
                            t.ExternalAffiliateId == s.ExternalAffiliateId
                            && t.SubjectId == s.Id
                            && t.Type == Domain.Enums.AffiliateTransactionType.Commission))
            .ToListAsync();

    public async Task<AffiliateTransaction?> GetTransactionByIdAsync(int transactionId)
        => await db.AffiliateTransactions.FirstOrDefaultAsync(t => t.Id == transactionId);

    public async Task DeleteTransactionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await db.Database.BeginTransactionAsync();
            try
            {
                db.ExternalAffiliates.Update(affiliate);
                db.AffiliateTransactions.Remove(transaction);
                await db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();
                throw new BusinessException("CONCURRENT_REQUEST",
                    "Eşzamanlı işlem tespit edildi. Lütfen tekrar deneyin.");
            }
        });
    }

    public async Task<bool> GrantCommissionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await db.Database.BeginTransactionAsync();
            try
            {
                db.ExternalAffiliates.Update(affiliate);
                db.AffiliateTransactions.Add(transaction);
                await db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();
                throw new BusinessException("CONCURRENT_REQUEST",
                    "Eşzamanlı işlem tespit edildi. Lütfen tekrar deneyin.");
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                await dbTransaction.RollbackAsync();
                return false;
            }
        });
    }

    public async Task RecordPaymentTransactionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await db.Database.BeginTransactionAsync();
            try
            {
                db.ExternalAffiliates.Update(affiliate);
                db.AffiliateTransactions.Add(transaction);
                await db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();
                throw new BusinessException("CONCURRENT_REQUEST",
                    "Eşzamanlı işlem tespit edildi. Lütfen tekrar deneyin.");
            }
        });
    }

    public async Task RecordAdjustmentTransactionAsync(ExternalAffiliate affiliate, AffiliateTransaction transaction)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await db.Database.BeginTransactionAsync();
            try
            {
                db.ExternalAffiliates.Update(affiliate);
                db.AffiliateTransactions.Add(transaction);
                await db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await dbTransaction.RollbackAsync();
                throw new BusinessException("CONCURRENT_REQUEST",
                    "Eşzamanlı işlem tespit edildi. Lütfen tekrar deneyin.");
            }
        });
    }

    /// <summary>
    /// PostgreSQL unique_violation error code: 23505
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        => ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505";
}
