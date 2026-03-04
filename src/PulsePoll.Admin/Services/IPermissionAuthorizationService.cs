namespace PulsePoll.Admin.Services;

public interface IPermissionAuthorizationService
{
    Task<bool> HasPermissionAsync(string permissionCode);
    Task<bool> HasAnyPermissionAsync(params string[] permissionCodes);
    Task<bool> HasAllPermissionsAsync(params string[] permissionCodes);
    Task<HashSet<string>> GetCurrentPermissionsAsync();
    void InvalidateCache();
}
