using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface INotificationDistributionConfigService
{
    Task<NotificationDistributionConfigDto> GetAsync();
    Task SaveAsync(NotificationDistributionConfigDto dto, int adminId);
}
