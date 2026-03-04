using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IAppContentConfigService
{
    Task<AppContentConfigDto> GetAsync();
    Task SaveAsync(AppContentConfigDto dto, int adminId);
}
