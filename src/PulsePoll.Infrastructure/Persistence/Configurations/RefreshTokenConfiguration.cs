using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasIndex(r => r.Token)
               .IsUnique()
               .HasDatabaseName("idx_refresh_tokens_token");

        builder.HasIndex(r => r.SubjectId)
               .HasDatabaseName("idx_refresh_tokens_subject_id");

        builder.HasIndex(r => r.ExpiresAt)
               .HasDatabaseName("idx_refresh_tokens_expires_at");

        builder.Property(r => r.Token).IsRequired().HasMaxLength(128);
        builder.Property(r => r.CreatedByIp).HasMaxLength(50);
        builder.Property(r => r.RevokedByIp).HasMaxLength(50);
        builder.Property(r => r.ReplacedByToken).HasMaxLength(128);
        builder.Property(r => r.RevokedReason).HasMaxLength(200);

        builder.HasOne(r => r.Subject)
               .WithMany()
               .HasForeignKey(r => r.SubjectId)
               .HasConstraintName("fk_refresh_tokens_subjects")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
