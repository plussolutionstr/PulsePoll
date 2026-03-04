using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class AppContentConfigService(IAppContentConfigRepository repository) : IAppContentConfigService
{
    public async Task<AppContentConfigDto> GetAsync()
    {
        var current = await repository.GetCurrentAsync();
        return current is null ? Default() : ToDto(current);
    }

    public async Task SaveAsync(AppContentConfigDto dto, int adminId)
    {
        Validate(dto);

        var entity = new AppContentConfig
        {
            KvkkText = dto.KvkkText.Trim(),
            ContactTitle = dto.ContactTitle.Trim(),
            ContactBody = dto.ContactBody.Trim(),
            ContactEmail = NormalizeNullable(dto.ContactEmail),
            ContactPhone = NormalizeNullable(dto.ContactPhone),
            ContactWhatsapp = NormalizeNullable(dto.ContactWhatsapp)
        };

        await repository.UpsertAsync(entity, adminId);
    }

    public static AppContentConfigDto Default()
        => new(
            KvkkText: "KVKK metni henüz tanımlanmadı.",
            ContactTitle: "Bize Ulaşın",
            ContactBody: string.Empty,
            ContactEmail: null,
            ContactPhone: null,
            ContactWhatsapp: null);

    private static AppContentConfigDto ToDto(AppContentConfig x)
        => new(
            x.KvkkText,
            x.ContactTitle,
            x.ContactBody,
            x.ContactEmail,
            x.ContactPhone,
            x.ContactWhatsapp);

    private static void Validate(AppContentConfigDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.KvkkText))
            throw new BusinessException("INVALID_KVKK_TEXT", "KVKK metni zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.ContactTitle))
            throw new BusinessException("INVALID_CONTACT_TITLE", "Bize Ulaşın başlığı zorunludur.");
    }

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
