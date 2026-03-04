using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.Property(x => x.QueueName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MessageType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.Property(x => x.OccurredAt).IsRequired();

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("idx_outbox_messages_processed_at");
        builder.HasIndex(x => x.OccurredAt)
            .HasDatabaseName("idx_outbox_messages_occurred_at");
        builder.HasIndex(x => x.LockedUntil)
            .HasDatabaseName("idx_outbox_messages_locked_until");
    }
}
