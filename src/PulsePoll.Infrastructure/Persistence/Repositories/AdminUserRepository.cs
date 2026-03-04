using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class AdminUserRepository(AppDbContext db) : IAdminUserRepository
{
    public Task<AdminUser?> GetByEmailAsync(string email)
        => db.AdminUsers.FirstOrDefaultAsync(a => a.Email == email && a.IsActive);

    public Task<AdminUser?> GetByIdAsync(int id)
        => db.AdminUsers
             .Include(a => a.AdminUserRoles).ThenInclude(ar => ar.Role)
             .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<List<string>> GetPermissionCodesAsync(int adminUserId)
    {
        return await db.AdminUserRoles
             .Where(ar =>
                 ar.AdminUserId == adminUserId &&
                 ar.Role.DeletedAt == null &&
                 ar.Role.IsActive)
             .SelectMany(ar => ar.Role.RolePermissions)
             .Select(rp => rp.Permission)
             .Where(p => p.IsActive && p.DeletedAt == null)
             .Select(p => p.Code)
             .Distinct()
             .ToListAsync();
    }

    public async Task<Dictionary<int, List<string>>> GetPermissionMapAsync()
    {
        var activeAdminUserIds = await db.AdminUsers
            .Where(a => a.DeletedAt == null && a.IsActive)
            .Select(a => a.Id)
            .ToListAsync();

        var permissionRows = await db.AdminUserRoles
            .Where(ar =>
                ar.AdminUser.DeletedAt == null &&
                ar.AdminUser.IsActive &&
                ar.Role.DeletedAt == null &&
                ar.Role.IsActive)
            .SelectMany(ar => ar.Role.RolePermissions
                .Where(rp => rp.Permission.DeletedAt == null && rp.Permission.IsActive)
                .Select(rp => new { ar.AdminUserId, rp.Permission.Code }))
            .ToListAsync();

        var map = permissionRows
            .GroupBy(x => x.AdminUserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Code)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList());

        foreach (var adminUserId in activeAdminUserIds)
        {
            map.TryAdd(adminUserId, []);
        }

        return map;
    }

    public async Task AddAsync(AdminUser user)
    {
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AdminUser user)
    {
        db.AdminUsers.Update(user);
        await db.SaveChangesAsync();
    }
}
