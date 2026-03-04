using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Summary).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(n => n.LinkUrl).HasMaxLength(500);

        builder.HasIndex(n => new { n.IsActive, n.StartsAt, n.EndsAt })
            .HasDatabaseName("idx_news_is_active_starts_at_ends_at");

        builder.HasIndex(n => n.Order)
            .HasDatabaseName("idx_news_order");

        builder.HasIndex(n => n.MediaAssetId)
            .HasDatabaseName("idx_news_media_asset_id");

        builder.HasOne(n => n.MediaAsset)
            .WithMany(a => a.News)
            .HasForeignKey(n => n.MediaAssetId)
            .HasConstraintName("fk_news_media_assets")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
