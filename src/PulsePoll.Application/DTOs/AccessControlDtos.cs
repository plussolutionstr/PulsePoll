namespace PulsePoll.Application.DTOs;

public record AdminUserListItemDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    List<int> RoleIds,
    string RoleNames,
    DateTime CreatedAt);

public record AdminUserEditDto(
    int? Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    List<int> RoleIds);

public record SaveAdminUserDto(
    int? Id,
    string FirstName,
    string LastName,
    string Email,
    string? Password,
    bool IsActive,
    List<int> RoleIds);

public record RoleListItemDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int UserCount,
    int PermissionCount,
    DateTime CreatedAt);

public record RoleEditDto(
    int? Id,
    string Name,
    string? Description,
    bool IsActive,
    List<int> PermissionIds);

public record SaveRoleDto(
    int? Id,
    string Name,
    string? Description,
    bool IsActive,
    List<int> PermissionIds);

public record PermissionItemDto(
    int Id,
    string Code,
    string Name,
    string Module,
    bool IsActive);

public record RoleLookupDto(
    int Id,
    string Name,
    bool IsActive);
