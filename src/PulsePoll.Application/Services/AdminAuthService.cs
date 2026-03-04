using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class AdminAuthService(
    IAdminUserRepository adminUserRepository,
    IAdminPermissionCacheService permissionCacheService,
    IPasswordHasher passwordHasher) : IAdminAuthService
{
    public async Task<AdminUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await adminUserRepository.GetByEmailAsync(email);
        if (user is null || !user.IsActive) return null;
        if (!passwordHasher.Verify(password, user.PasswordHash)) return null;
        return user;
    }

    public Task<List<string>> GetPermissionCodesAsync(int adminUserId)
        => permissionCacheService.GetPermissionCodesAsync(adminUserId);
}
