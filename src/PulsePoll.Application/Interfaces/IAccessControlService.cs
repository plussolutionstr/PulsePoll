using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IAccessControlService
{
    Task<List<AdminUserListItemDto>> GetAdminUsersAsync();
    Task<AdminUserEditDto?> GetAdminUserAsync(int id);
    Task SaveAdminUserAsync(SaveAdminUserDto dto, int actorId);

    Task<List<RoleListItemDto>> GetRolesAsync();
    Task<RoleEditDto?> GetRoleAsync(int id);
    Task SaveRoleAsync(SaveRoleDto dto, int actorId);
    Task DeleteRoleAsync(int roleId, int actorId);

    Task<List<PermissionItemDto>> GetPermissionsAsync();
    Task<List<RoleLookupDto>> GetRoleLookupsAsync();
}
