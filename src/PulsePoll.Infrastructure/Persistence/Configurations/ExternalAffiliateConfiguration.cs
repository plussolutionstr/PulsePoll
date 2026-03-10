using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class ExternalAffiliateConfiguration : IEntityTypeConfiguration<ExternalAffiliate>
{
    public void Configure(EntityTypeBuilder<ExternalAffiliate> builder)
    {
        builder.HasIndex(a => a.AffiliateCode)
               .IsUnique()
               .HasDatabaseName("idx_external_affiliates_affiliate_code");

        builder.HasIndex(a => a.Email)
               .HasDatabaseName("idx_external_affiliates_email");

        builder.Property(a => a.Balance).HasPrecision(18, 2);
        builder.Property(a => a.TotalEarned).HasPrecision(18, 2);
        builder.Property(a => a.TotalPaid).HasPrecision(18, 2);
        builder.Property(a => a.CommissionAmount).HasPrecision(18, 2);

        builder.Property(a => a.Version).IsRowVersion();
    }
}
