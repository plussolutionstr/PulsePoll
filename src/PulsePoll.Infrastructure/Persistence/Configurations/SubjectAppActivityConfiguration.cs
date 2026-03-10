using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SubjectAppActivityConfiguration : IEntityTypeConfiguration<SubjectAppActivity>
{
    public void Configure(EntityTypeBuilder<SubjectAppActivity> builder)
    {
        builder.Property(x => x.ActivityDate).HasColumnType("date");
        builder.Property(x => x.FirstOpenAt).HasColumnType("timestamp without time zone");
        builder.Property(x => x.LastSeenAt).HasColumnType("timestamp without time zone");
        builder.Property(x => x.Platform).HasMaxLength(40);
        builder.Property(x => x.AppVersion).HasMaxLength(30);
        builder.Property(x => x.DeviceIdHash).HasMaxLength(128);

        builder.HasIndex(x => new { x.SubjectId, x.ActivityDate })
            .IsUnique()
            .HasDatabaseName("idx_subject_app_activities_subject_date");

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .HasConstraintName("fk_subject_app_activities_subjects")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
