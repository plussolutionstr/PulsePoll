using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class StoryViewConfiguration : IEntityTypeConfiguration<StoryView>
{
    public void Configure(EntityTypeBuilder<StoryView> builder)
    {
        builder.Property(v => v.SeenAt).IsRequired();

        builder.HasIndex(v => new { v.SubjectId, v.StoryId })
            .IsUnique()
            .HasDatabaseName("ux_story_views_subject_story");

        builder.HasIndex(v => new { v.SubjectId, v.SeenAt })
            .HasDatabaseName("idx_story_views_subject_seen_at");

        builder.HasOne(v => v.Subject)
            .WithMany()
            .HasForeignKey(v => v.SubjectId)
            .HasConstraintName("fk_story_views_subjects")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Story)
            .WithMany(s => s.StoryViews)
            .HasForeignKey(v => v.StoryId)
            .HasConstraintName("fk_story_views_stories")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
