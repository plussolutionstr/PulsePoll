using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Admin.Services;

// Permissions are stored in Redis permission map and loaded per admin user.
// Claims are used only as fallback if Redis map has no entry.
public class PermissionAuthorizationService : IPermissionAuthorizationService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IAdminPermissionCacheService _permissionCacheService;
    private readonly SemaphoreSlim _permissionsLock = new(1, 1);
    private HashSet<string>? _cachedPermissions;
    private int? _cachedAdminUserId;

    public PermissionAuthorizationService(
        AuthenticationStateProvider authStateProvider,
        IAdminPermissionCacheService permissionCacheService)
    {
        _authStateProvider = authStateProvider;
        _permissionCacheService = permissionCacheService;
    }

    public async Task<bool> HasPermissionAsync(string permissionCode)
    {
        var permissions = await GetCurrentPermissionsAsync();
        return permissions.Contains(permissionCode);
    }

    public async Task<bool> HasAnyPermissionAsync(params string[] permissionCodes)
    {
        var permissions = await GetCurrentPermissionsAsync();
        return permissionCodes.Any(permissions.Contains);
    }

    public async Task<bool> HasAllPermissionsAsync(params string[] permissionCodes)
    {
        var permissions = await GetCurrentPermissionsAsync();
        return permissionCodes.All(permissions.Contains);
    }

    public async Task<HashSet<string>> GetCurrentPermissionsAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated != true)
        {
            InvalidateCache();
            return [];
        }

        var userId = GetAdminUserId(state.User);
        if (userId is null)
        {
            InvalidateCache();
            return [];
        }

        if (_cachedAdminUserId == userId && _cachedPermissions is not null)
            return [.. _cachedPermissions];

        await _permissionsLock.WaitAsync();
        try
        {
            if (_cachedAdminUserId == userId && _cachedPermissions is not null)
                return [.. _cachedPermissions];

            var cachedPermissions = await _permissionCacheService.GetPermissionCodesAsync(userId.Value);
            _cachedAdminUserId = userId;
            _cachedPermissions = cachedPermissions.Count > 0
                ? cachedPermissions.ToHashSet(StringComparer.OrdinalIgnoreCase)
                : GetPermissionsFromClaims(state.User);

            return [.. _cachedPermissions];
        }
        finally
        {
            _permissionsLock.Release();
        }
    }

    public void InvalidateCache()
    {
        _cachedAdminUserId = null;
        _cachedPermissions = null;
    }

    private static int? GetAdminUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var adminUserId) ? adminUserId : null;
    }

    private static HashSet<string> GetPermissionsFromClaims(ClaimsPrincipal user)
        => user.Claims
            .Where(c => c.Type == "perm")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
