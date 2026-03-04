using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.HasIndex(r => r.Name).IsUnique().HasDatabaseName("idx_roles_name");
    }
}
