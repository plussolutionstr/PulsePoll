using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IAccessControlRepository
{
    Task<List<AdminUser>> GetAdminUsersWithRolesAsync();
    Task<AdminUser?> GetAdminUserWithRolesAsync(int id);
    Task<bool> ExistsAdminEmailAsync(string email, int? excludeId = null);
    Task<bool> HasAnyOtherActiveAdminAsync(int excludeAdminUserId);
    Task AddAdminUserAsync(AdminUser user, IReadOnlyCollection<int> roleIds);
    Task UpdateAdminUserAsync(AdminUser user, IReadOnlyCollection<int> roleIds);

    Task<List<Role>> GetRolesWithPermissionsAsync();
    Task<Role?> GetRoleWithPermissionsAsync(int id);
    Task<bool> ExistsRoleNameAsync(string name, int? excludeId = null);
    Task<bool> IsRoleAssignedAsync(int roleId);
    Task AddRoleAsync(Role role, IReadOnlyCollection<int> permissionIds);
    Task UpdateRoleAsync(Role role, IReadOnlyCollection<int> permissionIds);
    Task DeleteRoleAsync(Role role);

    Task<List<Permission>> GetActivePermissionsAsync();
    Task<List<Role>> GetActiveRolesAsync();
}
