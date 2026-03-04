using PulsePoll.Application.Interfaces;

namespace PulsePoll.Application.Services;

public class AdminPermissionCacheService(
    ICacheService cache,
    IAdminUserRepository adminUserRepository) : IAdminPermissionCacheService
{
    private const string PermissionMapKey = "admin:permissions:map";

    public async Task<List<string>> GetPermissionCodesAsync(int adminUserId)
    {
        try
        {
            var map = await GetOrBuildPermissionMapAsync();
            return map.TryGetValue(adminUserId, out var permissions)
                ? permissions
                : [];
        }
        catch
        {
            return await adminUserRepository.GetPermissionCodesAsync(adminUserId);
        }
    }

    public async Task RefreshAsync()
    {
        var map = await adminUserRepository.GetPermissionMapAsync();
        try
        {
            await cache.SetAsync(map, PermissionMapKey);
        }
        catch
        {
            // Redis geçici olarak kapalıysa warm-up/refresh'i düşürmeyiz.
        }
    }

    private async Task<Dictionary<int, List<string>>> GetOrBuildPermissionMapAsync()
    {
        try
        {
            var cached = await cache.GetAsync<Dictionary<int, List<string>>>(PermissionMapKey);
            if (cached is not null)
                return cached;

            var map = await adminUserRepository.GetPermissionMapAsync();
            await cache.SetAsync(map, PermissionMapKey);
            return map;
        }
        catch
        {
            return await adminUserRepository.GetPermissionMapAsync();
        }
    }
}
