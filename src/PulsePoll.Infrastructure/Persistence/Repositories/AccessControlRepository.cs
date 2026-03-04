using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class AccessControlRepository(AppDbContext db) : IAccessControlRepository
{
    public Task<List<AdminUser>> GetAdminUsersWithRolesAsync()
        => db.AdminUsers
            .Where(x => x.DeletedAt == null)
            .Include(x => x.AdminUserRoles)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x.RolePermissions)
            .AsNoTracking()
            .ToListAsync();

    public Task<AdminUser?> GetAdminUserWithRolesAsync(int id)
        => db.AdminUsers
            .Where(x => x.DeletedAt == null)
            .Include(x => x.AdminUserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> ExistsAdminEmailAsync(string email, int? excludeId = null)
        => db.AdminUsers.AnyAsync(x =>
            x.DeletedAt == null &&
            x.Email == email &&
            (!excludeId.HasValue || x.Id != excludeId.Value));

    public Task<bool> HasAnyOtherActiveAdminAsync(int excludeAdminUserId)
        => db.AdminUsers.AnyAsync(x =>
            x.DeletedAt == null &&
            x.IsActive &&
            x.Id != excludeAdminUserId);

    public async Task AddAdminUserAsync(AdminUser user, IReadOnlyCollection<int> roleIds)
    {
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();

        await ReplaceUserRolesAsync(user.Id, roleIds);
    }

    public async Task UpdateAdminUserAsync(AdminUser user, IReadOnlyCollection<int> roleIds)
    {
        db.AdminUsers.Update(user);
        await ReplaceUserRolesAsync(user.Id, roleIds);
    }

    public Task<List<Role>> GetRolesWithPermissionsAsync()
        => db.Roles
            .Where(x => x.DeletedAt == null)
            .Include(x => x.AdminUserRoles)
            .ThenInclude(x => x.AdminUser)
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .AsNoTracking()
            .ToListAsync();

    public Task<Role?> GetRoleWithPermissionsAsync(int id)
        => db.Roles
            .Where(x => x.DeletedAt == null)
            .Include(x => x.AdminUserRoles)
            .ThenInclude(x => x.AdminUser)
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<bool> ExistsRoleNameAsync(string name, int? excludeId = null)
        => db.Roles.AnyAsync(x =>
            x.DeletedAt == null &&
            x.Name == name &&
            (!excludeId.HasValue || x.Id != excludeId.Value));

    public Task<bool> IsRoleAssignedAsync(int roleId)
        => db.AdminUserRoles.AnyAsync(x => x.RoleId == roleId);

    public async Task AddRoleAsync(Role role, IReadOnlyCollection<int> permissionIds)
    {
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        await ReplaceRolePermissionsAsync(role.Id, permissionIds);
    }

    public async Task UpdateRoleAsync(Role role, IReadOnlyCollection<int> permissionIds)
    {
        db.Roles.Update(role);
        await ReplaceRolePermissionsAsync(role.Id, permissionIds);
    }

    public async Task DeleteRoleAsync(Role role)
    {
        db.Roles.Update(role);
        await db.SaveChangesAsync();
    }

    public Task<List<Permission>> GetActivePermissionsAsync()
        => db.Permissions
            .Where(x => x.DeletedAt == null && x.IsActive)
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Name)
            .AsNoTracking()
            .ToListAsync();

    public Task<List<Role>> GetActiveRolesAsync()
        => db.Roles
            .Where(x => x.DeletedAt == null && x.IsActive)
            .OrderBy(x => x.Name)
            .AsNoTracking()
            .ToListAsync();

    private async Task ReplaceUserRolesAsync(int adminUserId, IReadOnlyCollection<int> roleIds)
    {
        var existing = await db.AdminUserRoles
            .Where(x => x.AdminUserId == adminUserId)
            .ToListAsync();

        db.AdminUserRoles.RemoveRange(existing);
        if (roleIds.Count > 0)
        {
            db.AdminUserRoles.AddRange(roleIds.Select(roleId => new AdminUserRole
            {
                AdminUserId = adminUserId,
                RoleId = roleId
            }));
        }

        await db.SaveChangesAsync();
    }

    private async Task ReplaceRolePermissionsAsync(int roleId, IReadOnlyCollection<int> permissionIds)
    {
        var existing = await db.RolePermissions
            .Where(x => x.RoleId == roleId)
            .ToListAsync();

        db.RolePermissions.RemoveRange(existing);
        if (permissionIds.Count > 0)
        {
            db.RolePermissions.AddRange(permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            }));
        }

        await db.SaveChangesAsync();
    }
}
