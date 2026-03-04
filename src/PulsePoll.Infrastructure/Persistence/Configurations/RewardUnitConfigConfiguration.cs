using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class RewardUnitConfigConfiguration : IEntityTypeConfiguration<RewardUnitConfig>
{
    public void Configure(EntityTypeBuilder<RewardUnitConfig> builder)
    {
        builder.Property(x => x.UnitCode)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.UnitLabel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TryMultiplier).HasPrecision(18, 6);
    }
}
