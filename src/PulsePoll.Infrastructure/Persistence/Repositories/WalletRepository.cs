using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using System.Text.Json;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class WalletRepository(AppDbContext db) : IWalletRepository
{
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync()
    {
        _transaction = await db.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public Task<Wallet?> GetBySubjectIdAsync(int subjectId)
        => db.Wallets.FirstOrDefaultAsync(w => w.SubjectId == subjectId);

    public async Task AddAsync(Wallet wallet)
    {
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        db.Wallets.Update(wallet);
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

    public async Task<int> CreateWithdrawalTransactionAsync(
        int subjectId,
        int bankAccountId,
        decimal amount,
        decimal amountTry,
        string unitCode,
        string unitLabel,
        decimal unitTryMultiplier,
        DateTime requestedAt,
        string referenceId,
        string description)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.SubjectId == subjectId)
            ?? throw new NotFoundException("Cüzdan");

        if (wallet.Balance < amount)
            throw new BusinessException("INSUFFICIENT_BALANCE", "Yetersiz bakiye.");

        wallet.Balance -= amount;
        wallet.SetUpdated(subjectId);

        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            Amount = amount,
            Type = Domain.Enums.WalletTransactionType.Withdrawal,
            ReferenceId = referenceId,
            Description = description
        };
        transaction.SetCreated(subjectId);
        db.WalletTransactions.Add(transaction);

        try
        {
            await db.SaveChangesAsync();

            var message = new WithdrawalRequestedMessage(
                subjectId,
                bankAccountId,
                amount,
                amountTry,
                unitCode,
                unitLabel,
                unitTryMultiplier,
                transaction.Id,
                requestedAt);

            var outboxMessage = new OutboxMessage
            {
                QueueName = Queues.WithdrawalRequested,
                MessageType = nameof(WithdrawalRequestedMessage),
                Payload = JsonSerializer.Serialize(message),
                OccurredAt = TurkeyTime.Now
            };
            db.OutboxMessages.Add(outboxMessage);

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new BusinessException("CONCURRENT_REQUEST",
                "Eşzamanlı işlem tespit edildi. Lütfen tekrar deneyin.");
        }

        return transaction.Id;
    }

    public async Task AddTransactionAsync(WalletTransaction transaction)
    {
        db.WalletTransactions.Add(transaction);
        await db.SaveChangesAsync();
    }

    public Task<WalletTransaction?> GetTransactionByIdAsync(int walletId, int transactionId)
        => db.WalletTransactions.FirstOrDefaultAsync(t => t.WalletId == walletId && t.Id == transactionId);

    public Task<WalletTransaction?> GetTransactionByReferenceAsync(int walletId, string referenceId)
        => db.WalletTransactions.FirstOrDefaultAsync(t => t.WalletId == walletId && t.ReferenceId == referenceId);

    public Task<List<WalletTransaction>> GetTransactionsAsync(int walletId, int skip, int take)
        => db.WalletTransactions
             .Where(t => t.WalletId == walletId)
             .OrderByDescending(t => t.CreatedAt)
             .Skip(skip)
             .Take(take)
             .ToListAsync();

    public async Task<List<(WalletTransaction Transaction, decimal RunningBalance)>> GetLedgerWithBalanceAsync(
        int walletId, int skip, int take)
    {
        // Get the paged transactions via EF
        var transactions = await db.WalletTransactions
             .Where(t => t.WalletId == walletId)
             .OrderByDescending(t => t.CreatedAt)
             .ThenByDescending(t => t.Id)
             .Skip(skip)
             .Take(take)
             .ToListAsync();

        if (transactions.Count == 0)
            return [];

        // Compute running balances via SQL window function, filter to page IDs
        var ids = transactions.Select(t => t.Id).ToArray();
        var connection = db.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

        if (shouldCloseConnection)
            await db.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                WITH balances AS (
                    SELECT t.id,
                           SUM(CASE WHEN t.type = 0 THEN t.amount ELSE -t.amount END)
                               OVER (ORDER BY t.created_at, t.id) AS running_balance
                    FROM wallet_transactions t
                    WHERE t.wallet_id = @walletId
                )
                SELECT b.id, b.running_balance
                FROM balances b
                WHERE b.id = ANY(@ids)
                """;

            var walletIdParam = cmd.CreateParameter();
            walletIdParam.ParameterName = "walletId";
            walletIdParam.Value = walletId;
            cmd.Parameters.Add(walletIdParam);

            var idsParam = cmd.CreateParameter();
            idsParam.ParameterName = "ids";
            idsParam.Value = ids;
            cmd.Parameters.Add(idsParam);

            var balanceMap = new Dictionary<int, decimal>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                balanceMap[reader.GetInt32(0)] = reader.GetDecimal(1);
            }

            return transactions
                .Select(tx => (tx, balanceMap.GetValueOrDefault(tx.Id)))
                .ToList();
        }
        finally
        {
            if (shouldCloseConnection)
                await db.Database.CloseConnectionAsync();
        }
    }

    public Task<int> CountTransactionsAsync(int walletId)
        => db.WalletTransactions.CountAsync(t => t.WalletId == walletId);

    public async Task DeleteTransactionAsync(WalletTransaction transaction)
    {
        db.WalletTransactions.Remove(transaction);
        await db.SaveChangesAsync();
    }

    public Task<List<BankAccount>> GetBankAccountsAsync(int subjectId)
        => db.BankAccounts
             .Where(b => b.SubjectId == subjectId && b.DeletedAt == null)
             .OrderByDescending(b => b.IsDefault)
             .ThenBy(b => b.CreatedAt)
             .ToListAsync();

    public Task<BankAccount?> GetBankAccountAsync(int subjectId, int accountId)
        => db.BankAccounts
             .FirstOrDefaultAsync(b => b.SubjectId == subjectId && b.Id == accountId && b.DeletedAt == null);

    public async Task AddBankAccountAsync(BankAccount account)
    {
        db.BankAccounts.Add(account);
        await db.SaveChangesAsync();
    }

    public async Task UpdateBankAccountAsync(BankAccount account)
    {
        db.BankAccounts.Update(account);
        await db.SaveChangesAsync();
    }

    public async Task DeleteBankAccountAsync(BankAccount account)
    {
        db.BankAccounts.Remove(account);
        await db.SaveChangesAsync();
    }
}
