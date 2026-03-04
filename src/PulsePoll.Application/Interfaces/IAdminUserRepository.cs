using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByEmailAsync(string email);
    Task<AdminUser?> GetByIdAsync(int id);
    Task<List<string>> GetPermissionCodesAsync(int adminUserId);
    Task<Dictionary<int, List<string>>> GetPermissionMapAsync();
    Task AddAsync(AdminUser user);
    Task UpdateAsync(AdminUser user);
}
