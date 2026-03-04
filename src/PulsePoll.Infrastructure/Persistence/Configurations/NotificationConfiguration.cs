using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.Type).HasMaxLength(50);
        builder.Property(n => n.DeliveryStatus)
               .HasConversion<int>()
               .HasDefaultValue(DeliveryStatus.Pending);
        builder.Property(n => n.ErrorMessage).HasMaxLength(500);

        builder.HasIndex(n => new { n.SubjectId, n.IsRead })
               .HasDatabaseName("idx_notifications_subject_id_is_read");

        builder.HasIndex(n => new { n.DeliveryStatus, n.CreatedAt })
               .HasDatabaseName("idx_notifications_status_created");

        builder.HasOne(n => n.Subject)
               .WithMany()
               .HasForeignKey(n => n.SubjectId)
               .HasConstraintName("fk_notifications_subjects")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
