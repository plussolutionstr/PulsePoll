using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.ObjectKey).IsRequired().HasMaxLength(500);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => a.ObjectKey).IsUnique().HasDatabaseName("uq_media_assets_object_key");
        builder.HasIndex(a => a.CreatedAt).HasDatabaseName("idx_media_assets_created_at");
    }
}
