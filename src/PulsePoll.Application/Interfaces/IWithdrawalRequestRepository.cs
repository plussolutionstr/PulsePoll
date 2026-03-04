using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IWithdrawalRequestRepository
{
    Task AddAsync(WithdrawalRequest request);
    Task<WithdrawalRequest?> GetByIdAsync(int id);
    Task<WithdrawalRequest?> GetByTransactionIdAsync(int walletTransactionId);
    Task<(List<WithdrawalRequest> Items, int Total)> GetPagedAsync(ApprovalStatus status, int skip, int take);
    Task<List<WithdrawalRequest>> GetApprovedWithoutBatchAsync();
    Task UpdateAsync(WithdrawalRequest request);
}
