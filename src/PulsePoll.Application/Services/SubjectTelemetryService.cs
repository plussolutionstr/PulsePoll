using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class SubjectTelemetryService(
    ISubjectRepository subjectRepository,
    ISubjectAppActivityRepository activityRepository,
    IReferralRewardService referralRewardService,
    IAffiliateRewardService affiliateRewardService,
    ILogger<SubjectTelemetryService> logger) : ISubjectTelemetryService
{
    public async Task ProcessActivityAsync(
        int subjectId,
        int activityType,
        string? platform,
        string? appVersion,
        string? deviceId,
        DateTime occurredAt,
        CancellationToken ct)
    {
        _ = await subjectRepository.GetByIdAsync(subjectId)
            ?? throw new NotFoundException("Denek");

        var today = DateOnly.FromDateTime(occurredAt);
        var type = (AppActivityType)activityType;
        var hashedDeviceId = HashDeviceId(deviceId);
        var truncatedPlatform = Truncate(platform, 40);
        var truncatedVersion = Truncate(appVersion, 30);

        var existing = await activityRepository.GetBySubjectAndDateAsync(subjectId, today, ct);

        if (existing is null)
        {
            var activity = new SubjectAppActivity
            {
                SubjectId = subjectId,
                ActivityDate = today,
                FirstOpenAt = occurredAt,
                LastSeenAt = occurredAt,
                OpenCount = type == AppActivityType.Open ? 1 : 0,
                TotalMinutes = 0,
                Platform = truncatedPlatform,
                AppVersion = truncatedVersion,
                DeviceIdHash = hashedDeviceId
            };
            activity.SetCreated(subjectId);

            await activityRepository.AddAsync(activity, ct);
        }
        else
        {
            existing.LastSeenAt = occurredAt;
            if (type == AppActivityType.Open)
                existing.OpenCount++;
            else if (type == AppActivityType.Heartbeat)
                existing.TotalMinutes += 5;
            if (truncatedPlatform is not null)
                existing.Platform = truncatedPlatform;
            if (truncatedVersion is not null)
                existing.AppVersion = truncatedVersion;
            if (hashedDeviceId is not null)
                existing.DeviceIdHash = hashedDeviceId;

            existing.SetUpdated(subjectId);

            await activityRepository.UpdateAsync(existing, ct);
        }

        await referralRewardService.TryGrantAsync(
            subjectId,
            ReferralRewardTriggerType.ActiveDaysReached,
            actorId: 0);

        await affiliateRewardService.TryGrantAsync(
            subjectId,
            ReferralRewardTriggerType.ActiveDaysReached,
            actorId: 0);

        logger.LogInformation(
            "Denek aktivitesi işlendi: SubjectId={SubjectId} Type={Type} Date={Date}",
            subjectId, type, today);
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
