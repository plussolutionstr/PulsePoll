using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class RegistrationConfigService(
    IRegistrationConfigRepository repository) : IRegistrationConfigService
{
    public async Task<RegistrationConfigDto> GetAsync()
    {
        var current = await repository.GetCurrentAsync();
        return current is null ? Default() : ToDto(current);
    }

    public async Task SaveAsync(RegistrationConfigDto dto, int adminId)
    {
        var entity = new RegistrationConfig
        {
            AutoApproveNewSubjects = dto.AutoApproveNewSubjects
        };

        await repository.UpsertAsync(entity, adminId);
    }

    public static RegistrationConfigDto Default()
        => new(AutoApproveNewSubjects: true);

    private static RegistrationConfigDto ToDto(RegistrationConfig config)
        => new(config.AutoApproveNewSubjects);
}
