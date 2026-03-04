using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SubjectScoreSnapshotConfiguration : IEntityTypeConfiguration<SubjectScoreSnapshot>
{
    public void Configure(EntityTypeBuilder<SubjectScoreSnapshot> builder)
    {
        builder.Property(x => x.Score).HasPrecision(6, 2);
        builder.Property(x => x.CoreScore).HasPrecision(6, 2);
        builder.Property(x => x.ActivityMultiplier).HasPrecision(6, 3);
        builder.Property(x => x.CalculatedAt).HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.SubjectId)
            .IsUnique()
            .HasDatabaseName("ux_subject_score_snapshots_subject_id");

        builder.HasOne(x => x.Subject)
            .WithOne(s => s.ScoreSnapshot)
            .HasForeignKey<SubjectScoreSnapshot>(x => x.SubjectId)
            .HasConstraintName("fk_subject_score_snapshots_subjects")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

