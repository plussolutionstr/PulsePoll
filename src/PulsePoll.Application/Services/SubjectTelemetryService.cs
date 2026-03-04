using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class SubjectTelemetryService(
    ISubjectRepository subjectRepository,
    ISubjectAppActivityRepository activityRepository,
    IReferralRewardService referralRewardService,
    ILogger<SubjectTelemetryService> logger) : ISubjectTelemetryService
{
    public async Task TrackActivityAsync(int subjectId, TrackSubjectActivityDto dto)
    {
        _ = await subjectRepository.GetByIdAsync(subjectId)
            ?? throw new NotFoundException("Denek");

        var activity = new SubjectAppActivity
        {
            SubjectId = subjectId,
            Type = dto.Type,
            OccurredAt = DateTime.UtcNow,
            Platform = Truncate(dto.Platform, 40),
            AppVersion = Truncate(dto.AppVersion, 30),
            DeviceIdHash = HashDeviceId(dto.DeviceId)
        };
        activity.SetCreated(subjectId);

        await activityRepository.AddAsync(activity);

        await referralRewardService.TryGrantAsync(
            subjectId,
            ReferralRewardTriggerType.ActiveDaysReached,
            actorId: 0);

        logger.LogInformation(
            "Subject app activity tracked: SubjectId={SubjectId} Type={Type}",
            subjectId, dto.Type);
    }

    private static string? HashDeviceId(string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return null;

        var bytes = Encoding.UTF8.GetBytes(deviceId.Trim());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
