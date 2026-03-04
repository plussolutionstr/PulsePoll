using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SpecialDayConfiguration : IEntityTypeConfiguration<SpecialDay>
{
    public void Configure(EntityTypeBuilder<SpecialDay> builder)
    {
        builder.Property(x => x.EventCode).HasMaxLength(80);
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Source).HasMaxLength(50);
        builder.Property(x => x.Category).HasConversion<int>();
        builder.Property(x => x.Date).HasColumnType("date");

        builder.HasIndex(x => x.Date)
            .HasDatabaseName("idx_special_days_date");

        builder.HasIndex(x => new { x.EventCode, x.Date })
            .IsUnique()
            .HasDatabaseName("uq_special_days_event_code_date");
    }
}
