using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IAdminAuthService
{
    Task<AdminUser?> ValidateCredentialsAsync(string email, string password);
    Task<List<string>> GetPermissionCodesAsync(int adminUserId);
}
