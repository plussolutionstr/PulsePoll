using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface ICommunicationAutomationConfigService
{
    Task<CommunicationAutomationConfigDto> GetAsync();
    Task SaveAsync(CommunicationAutomationConfigDto dto, int adminId);
}
