using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SubjectAppActivityConfiguration : IEntityTypeConfiguration<SubjectAppActivity>
{
    public void Configure(EntityTypeBuilder<SubjectAppActivity> builder)
    {
        builder.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Platform).HasMaxLength(40);
        builder.Property(x => x.AppVersion).HasMaxLength(30);
        builder.Property(x => x.DeviceIdHash).HasMaxLength(128);

        builder.HasIndex(x => new { x.SubjectId, x.OccurredAt })
            .HasDatabaseName("idx_subject_app_activities_subject_id_occurred_at");

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .HasConstraintName("fk_subject_app_activities_subjects")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

