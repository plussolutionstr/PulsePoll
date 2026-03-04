using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using System.Text.Json;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class WalletRepository(AppDbContext db) : IWalletRepository
{
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
                OccurredAt = DateTime.UtcNow
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

    public async Task DeleteBankAccountAsync(BankAccount account)
    {
        db.BankAccounts.Remove(account);
        await db.SaveChangesAsync();
    }
}
