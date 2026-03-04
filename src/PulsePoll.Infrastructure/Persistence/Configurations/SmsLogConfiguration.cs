using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
{
    public void Configure(EntityTypeBuilder<SmsLog> builder)
    {
        builder.Property(s => s.DeliveryStatus)
            .HasConversion<int>()
            .HasDefaultValue(DeliveryStatus.Sent);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(500);

        builder.HasOne(s => s.Subject)
               .WithMany()
               .HasForeignKey(s => s.SubjectId)
               .HasConstraintName("fk_sms_logs_subjects")
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => new { s.DeliveryStatus, s.CreatedAt })
               .HasDatabaseName("idx_sms_logs_status_created");

        builder.HasIndex(s => s.PhoneNumber)
               .HasDatabaseName("idx_sms_logs_phone");
    }
}
