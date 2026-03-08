using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class NotificationDistributionConfigConfiguration : IEntityTypeConfiguration<NotificationDistributionConfig>
{
    public void Configure(EntityTypeBuilder<NotificationDistributionConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.HourlyLimit)
            .IsRequired()
            .HasDefaultValue(300);
    }
}
