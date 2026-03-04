using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class AdminUserRoleConfiguration : IEntityTypeConfiguration<AdminUserRole>
{
    public void Configure(EntityTypeBuilder<AdminUserRole> builder)
    {
        builder.HasKey(ar => new { ar.AdminUserId, ar.RoleId });

        builder.HasOne(ar => ar.AdminUser)
               .WithMany(a => a.AdminUserRoles)
               .HasForeignKey(ar => ar.AdminUserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ar => ar.Role)
               .WithMany(r => r.AdminUserRoles)
               .HasForeignKey(ar => ar.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
