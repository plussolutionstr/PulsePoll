using PulsePoll.Application.DTOs;
using PulsePoll.Application.Models;

namespace PulsePoll.Application.Interfaces;

public interface IWalletService
{
    Task<WalletDto> GetBySubjectIdAsync(int subjectId);
    Task CreditAsync(int subjectId, decimal amount, string referenceId, string description);
    Task AddManualTransactionAsync(int subjectId, decimal amount, string description, int adminId);
    Task DeleteManualTransactionAsync(int subjectId, int transactionId, int adminId);
    Task RequestWithdrawalAsync(int subjectId, WithdrawalRequestDto dto);
    Task ApproveWithdrawalAsync(int requestId, int adminId);
    Task RejectWithdrawalAsync(int requestId, string reason, int adminId);
    Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(int subjectId, int page = 1, int pageSize = 20);
    Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(int subjectId);
    Task AddBankAccountAsync(int subjectId, AddBankAccountDto dto);
    Task DeleteBankAccountAsync(int subjectId, int accountId);
    Task<List<WalletLedgerDto>> GetLedgerAsync(int subjectId, int page = 1, int pageSize = 50);
}
