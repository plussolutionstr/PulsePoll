using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class NotificationDistributionConfigService(
    INotificationDistributionConfigRepository repository) : INotificationDistributionConfigService
{
    public async Task<NotificationDistributionConfigDto> GetAsync()
    {
        var current = await repository.GetCurrentAsync();
        return current is null ? Default() : ToDto(current);
    }

    public async Task SaveAsync(NotificationDistributionConfigDto dto, int adminId)
    {
        Validate(dto);
        var entity = new NotificationDistributionConfig
        {
            HourlyLimit = dto.HourlyLimit
        };
        await repository.UpsertAsync(entity, adminId);
    }

    public static NotificationDistributionConfigDto Default() => new(HourlyLimit: 300);

    private static NotificationDistributionConfigDto ToDto(NotificationDistributionConfig x)
        => new(x.HourlyLimit);

    private static void Validate(NotificationDistributionConfigDto dto)
    {
        if (dto.HourlyLimit < 1)
            throw new BusinessException("INVALID_HOURLY_LIMIT", "Saatlik limit en az 1 olmalıdır.");

        if (dto.HourlyLimit > 10000)
            throw new BusinessException("INVALID_HOURLY_LIMIT", "Saatlik limit en fazla 10.000 olabilir.");
    }
}
