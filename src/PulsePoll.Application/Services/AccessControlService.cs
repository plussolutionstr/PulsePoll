using System.Reflection;
using PulsePoll.Application.Constants;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class AccessControlService(
    IAccessControlRepository repository,
    IAdminPermissionCacheService permissionCacheService,
    ICacheService cacheService,
    IPasswordHasher passwordHasher) : IAccessControlService
{
    private const string RolesCacheKey = "admin:access-control:roles";
    private const string PermissionsCacheKey = "admin:access-control:permissions:v2";
    private const string RoleLookupsCacheKey = "admin:access-control:role-lookups";

    private static readonly HashSet<string> KnownPermissionCodes = typeof(PermissionCodes)
        .GetNestedTypes(BindingFlags.Public)
        .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
        .Select(field => field.GetRawConstantValue() as string)
        .Where(code => !string.IsNullOrWhiteSpace(code))
        .Select(code => code!)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task<List<AdminUserListItemDto>> GetAdminUsersAsync()
    {
        var rows = await repository.GetAdminUsersWithRolesAsync();
        return rows
            .OrderByDescending(x => x.Id)
            .Select(x => new AdminUserListItemDto(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.IsActive,
                x.AdminUserRoles
                    .Where(ur => ur.Role.DeletedAt == null)
                    .Select(ur => ur.RoleId)
                    .Distinct()
                    .ToList(),
                string.Join(", ", x.AdminUserRoles
                    .Where(ur => ur.Role.DeletedAt == null)
                    .Select(ur => ur.Role.Name)
                    .Distinct()
                    .OrderBy(n => n)),
                x.CreatedAt))
            .ToList();
    }

    public async Task<AdminUserEditDto?> GetAdminUserAsync(int id)
    {
        var row = await repository.GetAdminUserWithRolesAsync(id);
        if (row is null)
            return null;

        return new AdminUserEditDto(
            row.Id,
            row.FirstName,
            row.LastName,
            row.Email,
            row.IsActive,
            row.AdminUserRoles
                .Where(ur => ur.Role.DeletedAt == null)
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToList());
    }

    public async Task SaveAdminUserAsync(SaveAdminUserDto dto, int actorId)
    {
        ValidateAdminUser(dto);

        var roleIds = dto.RoleIds.Distinct().ToList();
        var activeRoles = await repository.GetActiveRolesAsync();
        var activeRoleIdSet = activeRoles.Select(x => x.Id).ToHashSet();
        if (roleIds.Any(id => !activeRoleIdSet.Contains(id)))
            throw new BusinessException("INVALID_ROLE", "Seçilen rollerden en az biri geçersiz veya pasif.");

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await repository.ExistsAdminEmailAsync(normalizedEmail, dto.Id))
            throw new BusinessException("ADMIN_EMAIL_EXISTS", "Bu e-posta ile kayıtlı bir admin kullanıcı zaten mevcut.");

        if (dto.Id is null)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new BusinessException("PASSWORD_REQUIRED", "Yeni kullanıcı için şifre zorunludur.");

            var user = new AdminUser
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHasher.Hash(dto.Password.Trim()),
                IsActive = dto.IsActive
            };
            user.SetCreated(actorId);

            await repository.AddAdminUserAsync(user, roleIds);
            await RefreshAccessControlCacheAsync();
            return;
        }

        var existing = await repository.GetAdminUserWithRolesAsync(dto.Id.Value)
            ?? throw new NotFoundException("Admin kullanıcı");

        if (existing.IsActive && !dto.IsActive)
        {
            var hasOtherActive = await repository.HasAnyOtherActiveAdminAsync(existing.Id);
            if (!hasOtherActive)
                throw new BusinessException("LAST_ACTIVE_ADMIN", "Son aktif admin kullanıcı pasif yapılamaz.");
        }

        existing.FirstName = dto.FirstName.Trim();
        existing.LastName = dto.LastName.Trim();
        existing.Email = normalizedEmail;
        existing.IsActive = dto.IsActive;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            existing.PasswordHash = passwordHasher.Hash(dto.Password.Trim());

        existing.SetUpdated(actorId);
        await repository.UpdateAdminUserAsync(existing, roleIds);
        await RefreshAccessControlCacheAsync();
    }

    public async Task<List<RoleListItemDto>> GetRolesAsync()
    {
        var cached = await cacheService.GetAsync<List<RoleListItemDto>>(RolesCacheKey);
        if (cached is not null)
            return cached;

        var rows = await repository.GetRolesWithPermissionsAsync();
        var result = rows
            .OrderBy(x => x.Name)
            .Select(x => new RoleListItemDto(
                x.Id,
                x.Name,
                x.Description,
                x.IsActive,
                x.AdminUserRoles.Count(ur => ur.AdminUser.IsActive),
                x.RolePermissions.Count(rp =>
                    rp.Permission.IsActive &&
                    rp.Permission.DeletedAt == null &&
                    KnownPermissionCodes.Contains(rp.Permission.Code)),
                x.CreatedAt))
            .ToList();

        await cacheService.SetAsync(result, RolesCacheKey);
        return result;
    }

    public async Task<RoleEditDto?> GetRoleAsync(int id)
    {
        var row = await repository.GetRoleWithPermissionsAsync(id);
        if (row is null)
            return null;

        return new RoleEditDto(
            row.Id,
            row.Name,
            row.Description,
            row.IsActive,
            row.RolePermissions
                .Where(rp => rp.Permission.IsActive && rp.Permission.DeletedAt == null)
                .Where(rp => KnownPermissionCodes.Contains(rp.Permission.Code))
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToList());
    }

    public async Task SaveRoleAsync(SaveRoleDto dto, int actorId)
    {
        ValidateRole(dto);

        var permissionIds = dto.PermissionIds.Distinct().ToList();
        var activePermissionIds = (await repository.GetActivePermissionsAsync())
            .Where(x => KnownPermissionCodes.Contains(x.Code))
            .Select(x => x.Id)
            .ToHashSet();
        if (permissionIds.Any(id => !activePermissionIds.Contains(id)))
            throw new BusinessException("INVALID_PERMISSION", "Seçilen yetkilerden en az biri geçersiz veya pasif.");

        var normalizedName = dto.Name.Trim();
        if (await repository.ExistsRoleNameAsync(normalizedName, dto.Id))
            throw new BusinessException("ROLE_NAME_EXISTS", "Bu isimde bir rol zaten mevcut.");

        if (dto.Id is null)
        {
            var role = new Role
            {
                Name = normalizedName,
                Description = NormalizeNullable(dto.Description),
                IsActive = dto.IsActive
            };
            role.SetCreated(actorId);
            await repository.AddRoleAsync(role, permissionIds);
            await RefreshAccessControlCacheAsync();
            return;
        }

        var existing = await repository.GetRoleWithPermissionsAsync(dto.Id.Value)
            ?? throw new NotFoundException("Rol");

        if (string.Equals(existing.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase) && !dto.IsActive)
            throw new BusinessException("ROLE_PROTECTED", "SuperAdmin rolü pasif yapılamaz.");

        existing.Name = normalizedName;
        existing.Description = NormalizeNullable(dto.Description);
        existing.IsActive = dto.IsActive;
        existing.SetUpdated(actorId);

        await repository.UpdateRoleAsync(existing, permissionIds);
        await RefreshAccessControlCacheAsync();
    }

    public async Task DeleteRoleAsync(int roleId, int actorId)
    {
        var role = await repository.GetRoleWithPermissionsAsync(roleId)
            ?? throw new NotFoundException("Rol");

        if (string.Equals(role.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("ROLE_PROTECTED", "SuperAdmin rolü silinemez.");

        if (await repository.IsRoleAssignedAsync(roleId))
            throw new BusinessException("ROLE_IN_USE", "Bu rol kullanıcıya atanmış olduğu için silinemez.");

        role.SetDeleted(actorId);
        await repository.DeleteRoleAsync(role);
        await RefreshAccessControlCacheAsync();
    }

    public async Task<List<PermissionItemDto>> GetPermissionsAsync()
    {
        var cached = await cacheService.GetAsync<List<PermissionItemDto>>(PermissionsCacheKey);
        if (cached is not null)
            return cached;

        var rows = await repository.GetActivePermissionsAsync();
        var result = rows
            .Where(x => KnownPermissionCodes.Contains(x.Code))
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Name)
            .Select(x => new PermissionItemDto(
                x.Id,
                x.Code,
                x.Name,
                x.Module,
                x.IsActive))
            .ToList();

        await cacheService.SetAsync(result, PermissionsCacheKey);
        return result;
    }

    public async Task<List<RoleLookupDto>> GetRoleLookupsAsync()
    {
        var cached = await cacheService.GetAsync<List<RoleLookupDto>>(RoleLookupsCacheKey);
        if (cached is not null)
            return cached;

        var rows = await repository.GetActiveRolesAsync();
        var result = rows
            .OrderBy(x => x.Name)
            .Select(x => new RoleLookupDto(x.Id, x.Name, x.IsActive))
            .ToList();

        await cacheService.SetAsync(result, RoleLookupsCacheKey);
        return result;
    }

    private static void ValidateAdminUser(SaveAdminUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new BusinessException("FIRST_NAME_REQUIRED", "Ad zorunludur.");
        if (string.IsNullOrWhiteSpace(dto.LastName))
            throw new BusinessException("LAST_NAME_REQUIRED", "Soyad zorunludur.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new BusinessException("EMAIL_REQUIRED", "E-posta zorunludur.");
        if (dto.RoleIds.Count == 0)
            throw new BusinessException("ROLE_REQUIRED", "En az bir rol seçmelisiniz.");
    }

    private static void ValidateRole(SaveRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessException("ROLE_NAME_REQUIRED", "Rol adı zorunludur.");
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task RefreshAccessControlCacheAsync()
    {
        await permissionCacheService.RefreshAsync();
        await cacheService.RemoveAsync(RolesCacheKey);
        await cacheService.RemoveAsync(PermissionsCacheKey);
        await cacheService.RemoveAsync(RoleLookupsCacheKey);
    }
}
