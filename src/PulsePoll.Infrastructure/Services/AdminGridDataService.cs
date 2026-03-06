using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using PulsePoll.Infrastructure.Persistence;

namespace PulsePoll.Infrastructure.Services;

public class AdminGridDataService(AppDbContext db) : IAdminGridDataService
{
    public IQueryable<Subject> GetSubjectsQuery(bool includeScoreSnapshot)
    {
        IQueryable<Subject> query = db.Subjects;
        query = query
            .Include(s => s.City)
            .Include(s => s.District)
            .Include(s => s.Profession)
            .Include(s => s.EducationLevel)
            .Include(s => s.HeadOfFamilyProfession)
            .Include(s => s.HeadOfFamilyEducationLevel)
            .Include(s => s.SocioeconomicStatus);

        if (includeScoreSnapshot)
            query = query.Include(s => s.ScoreSnapshot);

        return query.AsNoTracking();
    }

    public async Task<Dictionary<int, List<BankAccount>>> GetBankAccountsBySubjectAsync(
        CancellationToken cancellationToken = default)
    {
        var bankAccounts = await db.BankAccounts
            .Where(b => b.DeletedAt == null)
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return bankAccounts
            .GroupBy(b => b.SubjectId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public IQueryable<WithdrawalRequest> GetPendingWithdrawalsQuery()
        => db.WithdrawalRequests
            .Include(w => w.Subject)
            .Include(w => w.BankAccount)
            .Where(w => w.Status == ApprovalStatus.Pending)
            .OrderByDescending(w => w.CreatedAt)
            .AsNoTracking();

    public IQueryable<WithdrawalRequest> GetApprovedWithoutBatchWithdrawalsQuery()
        => db.WithdrawalRequests
            .Include(w => w.Subject)
            .Include(w => w.BankAccount)
            .Where(w => w.Status == ApprovalStatus.Approved
                        && !db.PaymentBatchItems.Any(i =>
                            i.WithdrawalRequestId == w.Id &&
                            i.DeletedAt == null))
            .OrderByDescending(w => w.CreatedAt)
            .AsNoTracking();

    public Task<List<WithdrawalRequest>> GetAllPendingWithdrawalsAsync(CancellationToken cancellationToken = default)
        => db.WithdrawalRequests
            .Include(w => w.Subject)
            .Include(w => w.BankAccount)
            .AsNoTracking()
            .Where(w => w.Status == ApprovalStatus.Pending)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<int>> GetPendingWithdrawalIdsAsync(
        IEnumerable<int> selectedIds,
        CancellationToken cancellationToken = default)
    {
        var ids = selectedIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        return await db.WithdrawalRequests
            .AsNoTracking()
            .Where(w => w.Status == ApprovalStatus.Pending && ids.Contains(w.Id))
            .Select(w => w.Id)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<SmsLog> GetSmsLogsQuery()
        => db.SmsLogs
            .Include(s => s.Subject)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking();

    public IQueryable<Notification> GetPushNotificationsQuery()
        => db.Notifications
            .Include(n => n.Subject)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking();
}
