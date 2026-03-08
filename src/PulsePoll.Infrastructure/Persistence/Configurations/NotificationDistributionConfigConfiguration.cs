using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class NotificationDistributionConfigConfiguration : IEntityTypeConfiguration<NotificationDistributionConfig>
{
    public const int SingletonId = 1;

    public void Configure(EntityTypeBuilder<NotificationDistributionConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.HourlyLimit)
            .IsRequired()
            .HasDefaultValue(300);

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_notification_distribution_configs_singleton",
            "id = 1"));
    }
}
