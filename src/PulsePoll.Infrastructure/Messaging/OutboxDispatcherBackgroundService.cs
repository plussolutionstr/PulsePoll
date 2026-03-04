using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;
using PulsePoll.Infrastructure.Persistence;

namespace PulsePoll.Infrastructure.Messaging;

public class OutboxDispatcherBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxDispatcherBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private static readonly TimeSpan EmptyPollDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedCount = await DispatchBatchAsync(stoppingToken);
                if (processedCount == 0)
                    await Task.Delay(EmptyPollDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatch döngüsünde hata oluştu.");
                await Task.Delay(ErrorDelay, stoppingToken);
            }
        }
    }

    private async Task<int> DispatchBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        var now = DateTime.UtcNow;

        List<OutboxMessage> messages;
        await using (var lockTx = await db.Database.BeginTransactionAsync(ct))
        {
            messages = await db.OutboxMessages
                .FromSqlInterpolated($@"
                    SELECT * FROM outbox_messages
                    WHERE processed_at IS NULL
                      AND (locked_until IS NULL OR locked_until < {now})
                    ORDER BY occurred_at
                    LIMIT {BatchSize}
                    FOR UPDATE SKIP LOCKED")
                .ToListAsync(ct);

            if (messages.Count == 0)
            {
                await lockTx.CommitAsync(ct);
                return 0;
            }

            foreach (var message in messages)
                message.LockedUntil = now.Add(LockDuration);

            await db.SaveChangesAsync(ct);
            await lockTx.CommitAsync(ct);
        }

        foreach (var message in messages)
        {
            try
            {
                await DispatchMessageAsync(message, publisher, ct);
                message.ProcessedAt = DateTime.UtcNow;
                message.LockedUntil = null;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.RetryCount += 1;
                message.LastError = Truncate(ex.Message, 2000);
                message.LockedUntil = DateTime.UtcNow.AddSeconds(Math.Min(120, 5 * message.RetryCount));

                logger.LogError(ex,
                    "Outbox mesajı gönderilemedi. OutboxId={OutboxId} Type={Type} Retry={RetryCount}",
                    message.Id, message.MessageType, message.RetryCount);
            }
        }

        await db.SaveChangesAsync(ct);
        return messages.Count;
    }

    private static async Task DispatchMessageAsync(
        OutboxMessage message,
        IMessagePublisher publisher,
        CancellationToken ct)
    {
        switch (message.MessageType)
        {
            case nameof(WithdrawalRequestedMessage):
            {
                var payload = JsonSerializer.Deserialize<WithdrawalRequestedMessage>(message.Payload)
                    ?? throw new InvalidOperationException("WithdrawalRequestedMessage payload deserialize edilemedi.");
                await publisher.PublishAsync(payload, message.QueueName, ct);
                return;
            }
            default:
                throw new InvalidOperationException($"Desteklenmeyen outbox message type: {message.MessageType}");
        }
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
