using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class AffiliateTransactionConfiguration : IEntityTypeConfiguration<AffiliateTransaction>
{
    public void Configure(EntityTypeBuilder<AffiliateTransaction> builder)
    {
        builder.Property(t => t.Amount).HasPrecision(18, 2);

        builder.HasIndex(t => t.ReferenceId)
               .IsUnique()
               .HasFilter("reference_id IS NOT NULL")
               .HasDatabaseName("idx_affiliate_transactions_reference_id");

        builder.HasOne(t => t.ExternalAffiliate)
               .WithMany(a => a.Transactions)
               .HasForeignKey(t => t.ExternalAffiliateId)
               .HasConstraintName("fk_affiliate_transactions_external_affiliates")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Subject)
               .WithMany()
               .HasForeignKey(t => t.SubjectId)
               .HasConstraintName("fk_affiliate_transactions_subjects")
               .OnDelete(DeleteBehavior.SetNull);
    }
}
