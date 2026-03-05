using System.Globalization;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Messaging;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class CommunicationAutomationConfigService(
    ICommunicationAutomationConfigRepository repository,
    IMessagePublisher publisher,
    ILogger<CommunicationAutomationConfigService> logger) : ICommunicationAutomationConfigService
{
    private const string TimeFormat = "HH:mm";

    public async Task<CommunicationAutomationConfigDto> GetAsync()
    {
        var current = await repository.GetCurrentAsync();
        return current is null ? Default() : ToDto(current);
    }

    public async Task SaveAsync(CommunicationAutomationConfigDto dto, int adminId)
    {
        Validate(dto);

        var entity = new CommunicationAutomationConfig
        {
            DailyRunTime = dto.DailyRunTime.Trim(),
            TimeZoneId = dto.TimeZoneId.Trim()
        };

        await repository.UpsertAsync(entity, adminId);

        try
        {
            await publisher.PublishAsync(
                new CommunicationAutomationScheduleChangedMessage(
                    entity.DailyRunTime,
                    entity.TimeZoneId,
                    adminId,
                    TurkeyTime.Now),
                Queues.CommunicationAutomationScheduleChanged);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "İletişim otomasyon schedule güncelleme mesajı yayınlanamadı.");
        }
    }

    public static CommunicationAutomationConfigDto Default()
        => new(
            DailyRunTime: "09:00",
            TimeZoneId: "Europe/Istanbul");

    private static CommunicationAutomationConfigDto ToDto(CommunicationAutomationConfig x)
        => new(
            x.DailyRunTime,
            x.TimeZoneId);

    private static void Validate(CommunicationAutomationConfigDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DailyRunTime))
            throw new BusinessException("INVALID_DAILY_RUN_TIME", "Günlük çalışma saati zorunludur.");

        if (!TimeOnly.TryParseExact(dto.DailyRunTime.Trim(), TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new BusinessException("INVALID_DAILY_RUN_TIME", "Saat formatı HH:mm olmalıdır (ör. 09:00).");

        if (string.IsNullOrWhiteSpace(dto.TimeZoneId))
            throw new BusinessException("INVALID_TIMEZONE", "Zaman dilimi zorunludur.");

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(dto.TimeZoneId.Trim());
        }
        catch (Exception)
        {
            throw new BusinessException("INVALID_TIMEZONE", "Geçersiz zaman dilimi.");
        }
    }
}
