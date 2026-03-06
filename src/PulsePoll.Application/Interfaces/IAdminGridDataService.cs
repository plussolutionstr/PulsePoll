using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IAdminGridDataService
{
    IQueryable<Subject> GetSubjectsQuery(bool includeScoreSnapshot);
    Task<Dictionary<int, List<BankAccount>>> GetBankAccountsBySubjectAsync(CancellationToken cancellationToken = default);

    IQueryable<WithdrawalRequest> GetPendingWithdrawalsQuery();
    IQueryable<WithdrawalRequest> GetApprovedWithoutBatchWithdrawalsQuery();
    Task<List<WithdrawalRequest>> GetAllPendingWithdrawalsAsync(CancellationToken cancellationToken = default);
    Task<List<int>> GetPendingWithdrawalIdsAsync(IEnumerable<int> selectedIds, CancellationToken cancellationToken = default);

    IQueryable<SmsLog> GetSmsLogsQuery();
    IQueryable<Notification> GetPushNotificationsQuery();
}
