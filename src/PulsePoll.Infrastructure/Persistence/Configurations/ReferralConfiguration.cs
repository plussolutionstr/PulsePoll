using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.CommissionEarned).HasPrecision(18, 2);
        builder.Property(r => r.CommissionAmountTry).HasPrecision(18, 2);
        builder.Property(r => r.CommissionUnitCode).HasMaxLength(16);
        builder.Property(r => r.CommissionUnitLabel).HasMaxLength(20);
        builder.Property(r => r.CommissionUnitTryMultiplier).HasPrecision(18, 6);

        builder.HasOne(r => r.Referrer)
               .WithMany(s => s.ReferralsGiven)
               .HasForeignKey(r => r.ReferrerId)
               .HasConstraintName("fk_referrals_referrer")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReferredSubject)
               .WithMany(s => s.ReferredBy)
               .HasForeignKey(r => r.ReferredSubjectId)
               .HasConstraintName("fk_referrals_referred_subject")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.ReferrerId, r.ReferredSubjectId })
               .IsUnique()
               .HasDatabaseName("uq_referrals_referrer_referred");

        builder.HasIndex(r => r.ReferrerId)
               .HasDatabaseName("idx_referrals_referrer");

        builder.HasIndex(r => r.ReferredSubjectId)
               .IsUnique()
               .HasDatabaseName("uq_referrals_referred_subject");
    }
}
