using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IRegistrationConfigService
{
    Task<RegistrationConfigDto> GetAsync();
    Task SaveAsync(RegistrationConfigDto dto, int adminId);
}
