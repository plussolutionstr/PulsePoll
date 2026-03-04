using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.HasIndex(a => a.Email)
               .IsUnique()
               .HasDatabaseName("idx_admin_users_email");

        builder.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.LastName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(256);
        builder.Property(a => a.PasswordHash).IsRequired();
    }
}
