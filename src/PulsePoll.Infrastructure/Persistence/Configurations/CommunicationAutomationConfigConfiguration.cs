using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class CommunicationAutomationConfigConfiguration : IEntityTypeConfiguration<CommunicationAutomationConfig>
{
    public void Configure(EntityTypeBuilder<CommunicationAutomationConfig> builder)
    {
        builder.Property(x => x.DailyRunTime)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(x => x.TimeZoneId)
            .HasMaxLength(100)
            .IsRequired();
    }
}
