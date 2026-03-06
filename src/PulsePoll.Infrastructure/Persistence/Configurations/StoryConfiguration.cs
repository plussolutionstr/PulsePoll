using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(s => s.StoryImageUrl).HasMaxLength(500);
        builder.Property(s => s.LinkUrl).HasMaxLength(500);
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => new { s.IsActive, s.StartsAt, s.EndsAt })
               .HasDatabaseName("idx_stories_is_active_starts_at_ends_at");

        builder.HasIndex(s => s.Order)
               .HasDatabaseName("idx_stories_order");

        builder.HasIndex(s => s.MediaAssetId)
               .HasDatabaseName("idx_stories_media_asset_id");
        builder.HasIndex(s => s.StoryMediaAssetId)
               .HasDatabaseName("idx_stories_story_media_asset_id");

        builder.HasOne(s => s.MediaAsset)
               .WithMany(a => a.Stories)
               .HasForeignKey(s => s.MediaAssetId)
               .HasConstraintName("fk_stories_media_assets")
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.StoryMediaAsset)
               .WithMany(a => a.StoryImages)
               .HasForeignKey(s => s.StoryMediaAssetId)
               .HasConstraintName("fk_stories_story_media_assets")
               .OnDelete(DeleteBehavior.SetNull);
    }
}
