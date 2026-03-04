using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Module).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.HasIndex(p => p.Code).IsUnique().HasDatabaseName("idx_permissions_code");
    }
}
