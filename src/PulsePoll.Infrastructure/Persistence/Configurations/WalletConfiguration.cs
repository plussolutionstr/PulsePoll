using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.Property(w => w.Balance).HasPrecision(18, 2);
        builder.Property(w => w.TotalEarned).HasPrecision(18, 2);

        builder.Property(w => w.Version).IsRowVersion();

        builder.HasOne(w => w.Subject)
               .WithMany()
               .HasForeignKey(w => w.SubjectId)
               .HasConstraintName("fk_wallets_subjects")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Transactions)
               .WithOne(t => t.Wallet)
               .HasForeignKey(t => t.WalletId)
               .HasConstraintName("fk_wallet_transactions_wallets");
    }
}

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.ReferenceId).HasMaxLength(200);

        builder.HasIndex(t => t.WalletId)
               .HasDatabaseName("idx_wallet_transactions_wallet_id");
    }
}

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.Property(b => b.BankName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.IbanLast4).IsRequired().HasMaxLength(4);
        builder.Property(b => b.IbanEncrypted).IsRequired();

        builder.HasOne(b => b.Subject)
               .WithMany(s => s.BankAccounts)
               .HasForeignKey(b => b.SubjectId)
               .HasConstraintName("fk_bank_accounts_subjects")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
