using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class PaymentBatchConfiguration : IEntityTypeConfiguration<PaymentBatch>
{
    public void Configure(EntityTypeBuilder<PaymentBatch> builder)
    {
        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(30);
        builder.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Note).HasMaxLength(500);

        builder.HasIndex(b => b.BatchNumber).IsUnique().HasDatabaseName("uq_payment_batches_batch_number");
        builder.HasIndex(b => b.Status).HasDatabaseName("idx_payment_batches_status");
    }
}

public class PaymentBatchItemConfiguration : IEntityTypeConfiguration<PaymentBatchItem>
{
    public void Configure(EntityTypeBuilder<PaymentBatchItem> builder)
    {
        builder.Property(i => i.FailureReason).HasMaxLength(500);

        builder.HasOne(i => i.PaymentBatch)
               .WithMany(b => b.Items)
               .HasForeignKey(i => i.PaymentBatchId)
               .HasConstraintName("fk_payment_batch_items_batch")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.WithdrawalRequest)
               .WithMany()
               .HasForeignKey(i => i.WithdrawalRequestId)
               .HasConstraintName("fk_payment_batch_items_withdrawal")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.WithdrawalRequestId)
               .IsUnique()
               .HasDatabaseName("uq_payment_batch_items_withdrawal_request");
    }
}

public class PaymentSettingConfiguration : IEntityTypeConfiguration<PaymentSetting>
{
    public void Configure(EntityTypeBuilder<PaymentSetting> builder)
    {
        builder.Property(s => s.Key).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Value).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.Description).HasMaxLength(200);

        builder.HasIndex(s => s.Key).IsUnique().HasDatabaseName("uq_payment_settings_key");
    }
}
