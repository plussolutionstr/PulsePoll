using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IWalletRepository
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    Task<Wallet?> GetBySubjectIdAsync(int subjectId);
    Task AddAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
    Task<int> CreateWithdrawalTransactionAsync(
        int subjectId,
        int bankAccountId,
        decimal amount,
        decimal amountTry,
        string unitCode,
        string unitLabel,
        decimal unitTryMultiplier,
        DateTime requestedAt,
        string referenceId,
        string description);

    Task AddTransactionAsync(WalletTransaction transaction);
    Task<WalletTransaction?> GetTransactionByIdAsync(int walletId, int transactionId);
    Task<WalletTransaction?> GetTransactionByReferenceAsync(int walletId, string referenceId);
    Task<List<WalletTransaction>> GetTransactionsAsync(int walletId, int skip, int take);
    Task<int> CountTransactionsAsync(int walletId);
    Task DeleteTransactionAsync(WalletTransaction transaction);

    Task<List<BankAccount>> GetBankAccountsAsync(int subjectId);
    Task<BankAccount?> GetBankAccountAsync(int subjectId, int accountId);
    Task AddBankAccountAsync(BankAccount account);
    Task UpdateBankAccountAsync(BankAccount account);
    Task DeleteBankAccountAsync(BankAccount account);
}
