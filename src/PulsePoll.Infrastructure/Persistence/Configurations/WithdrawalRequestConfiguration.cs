using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.Property(w => w.Amount).HasPrecision(18, 2);
        builder.Property(w => w.AmountTry).HasPrecision(18, 2);
        builder.Property(w => w.UnitCode).HasMaxLength(16).IsRequired();
        builder.Property(w => w.UnitLabel).HasMaxLength(20).IsRequired();
        builder.Property(w => w.UnitTryMultiplier).HasPrecision(18, 6);
        builder.Property(w => w.RejectionReason).HasMaxLength(500);

        builder.HasIndex(w => w.WalletTransactionId)
               .IsUnique()
               .HasDatabaseName("ux_withdrawal_requests_wallet_transaction_id");

        builder.HasOne(w => w.Subject)
               .WithMany()
               .HasForeignKey(w => w.SubjectId)
               .HasConstraintName("fk_withdrawal_requests_subjects")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.BankAccount)
               .WithMany()
               .HasForeignKey(w => w.BankAccountId)
               .HasConstraintName("fk_withdrawal_requests_bank_accounts")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.WalletTransaction)
               .WithMany()
               .HasForeignKey(w => w.WalletTransactionId)
               .HasConstraintName("fk_withdrawal_requests_wallet_transactions")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
