namespace PulsePoll.Application.Interfaces;

public interface IAdminPermissionCacheService
{
    Task<List<string>> GetPermissionCodesAsync(int adminUserId);
    Task RefreshAsync();
}
