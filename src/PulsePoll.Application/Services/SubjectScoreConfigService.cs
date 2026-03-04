using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class SubjectScoreConfigService(ISubjectScoreConfigRepository repository) : ISubjectScoreConfigService
{
    public async Task<SubjectScoreConfigDto> GetAsync()
    {
        var current = await repository.GetCurrentAsync();
        return current is null ? Default() : ToDto(current);
    }

    public async Task SaveAsync(SubjectScoreConfigDto dto, int adminId)
    {
        Validate(dto);

        var entity = new SubjectScoreConfig
        {
            ParticipationWeight = dto.ParticipationWeight,
            CompletionWeight = dto.CompletionWeight,
            QualityWeight = dto.QualityWeight,
            ApprovalTrustWeight = dto.ApprovalTrustWeight,
            SpeedWeight = dto.SpeedWeight,
            ConfidencePivot = dto.ConfidencePivot,
            ScoreBaseline = dto.ScoreBaseline,
            Star1Max = dto.Star1Max,
            Star2Max = dto.Star2Max,
            Star3Max = dto.Star3Max,
            Star4Max = dto.Star4Max,
            VeryActiveLastSeenDays = dto.VeryActiveLastSeenDays,
            ActiveLastSeenDays = dto.ActiveLastSeenDays,
            WarmLastSeenDays = dto.WarmLastSeenDays,
            CoolingLastSeenDays = dto.CoolingLastSeenDays,
            VeryActiveMinDays30 = dto.VeryActiveMinDays30,
            VeryActiveMultiplier = dto.VeryActiveMultiplier,
            ActiveMultiplier = dto.ActiveMultiplier,
            WarmMultiplier = dto.WarmMultiplier,
            CoolingMultiplier = dto.CoolingMultiplier,
            DormantMultiplier = dto.DormantMultiplier,
            NoTelemetryMultiplier = dto.NoTelemetryMultiplier
        };

        await repository.UpsertAsync(entity, adminId);
    }

    public static SubjectScoreConfigDto Default()
        => new(
            ParticipationWeight: 0.25m,
            CompletionWeight: 0.30m,
            QualityWeight: 0.20m,
            ApprovalTrustWeight: 0.15m,
            SpeedWeight: 0.10m,
            ConfidencePivot: 20,
            ScoreBaseline: 60m,
            Star1Max: 44m,
            Star2Max: 59m,
            Star3Max: 74m,
            Star4Max: 89m,
            VeryActiveLastSeenDays: 3,
            ActiveLastSeenDays: 7,
            WarmLastSeenDays: 14,
            CoolingLastSeenDays: 30,
            VeryActiveMinDays30: 10,
            VeryActiveMultiplier: 1.15m,
            ActiveMultiplier: 1.08m,
            WarmMultiplier: 1.00m,
            CoolingMultiplier: 0.93m,
            DormantMultiplier: 0.85m,
            NoTelemetryMultiplier: 1.00m);

    private static SubjectScoreConfigDto ToDto(SubjectScoreConfig x)
        => new(
            x.ParticipationWeight,
            x.CompletionWeight,
            x.QualityWeight,
            x.ApprovalTrustWeight,
            x.SpeedWeight,
            x.ConfidencePivot,
            x.ScoreBaseline,
            x.Star1Max,
            x.Star2Max,
            x.Star3Max,
            x.Star4Max,
            x.VeryActiveLastSeenDays,
            x.ActiveLastSeenDays,
            x.WarmLastSeenDays,
            x.CoolingLastSeenDays,
            x.VeryActiveMinDays30,
            x.VeryActiveMultiplier,
            x.ActiveMultiplier,
            x.WarmMultiplier,
            x.CoolingMultiplier,
            x.DormantMultiplier,
            x.NoTelemetryMultiplier);

    private static void Validate(SubjectScoreConfigDto dto)
    {
        var sum = dto.ParticipationWeight + dto.CompletionWeight + dto.QualityWeight +
                  dto.ApprovalTrustWeight + dto.SpeedWeight;
        if (sum <= 0)
            throw new BusinessException("INVALID_WEIGHT_SUM", "Ağırlıklar toplamı 0'dan büyük olmalıdır.");

        if (dto.Star1Max < 0 || dto.Star2Max <= dto.Star1Max || dto.Star3Max <= dto.Star2Max ||
            dto.Star4Max <= dto.Star3Max || dto.Star4Max > 100)
            throw new BusinessException("INVALID_STAR_THRESHOLDS", "Yıldız eşikleri artan ve 0-100 aralığında olmalıdır.");

        if (dto.ConfidencePivot <= 0)
            throw new BusinessException("INVALID_CONFIDENCE_PIVOT", "Confidence pivot 0'dan büyük olmalıdır.");

        if (dto.VeryActiveLastSeenDays > dto.ActiveLastSeenDays ||
            dto.ActiveLastSeenDays > dto.WarmLastSeenDays ||
            dto.WarmLastSeenDays > dto.CoolingLastSeenDays)
            throw new BusinessException("INVALID_ACTIVITY_WINDOWS", "Aktivite gün eşikleri artan sırada olmalıdır.");

        if (dto.VeryActiveMultiplier <= 0 || dto.ActiveMultiplier <= 0 || dto.WarmMultiplier <= 0 ||
            dto.CoolingMultiplier <= 0 || dto.DormantMultiplier <= 0 || dto.NoTelemetryMultiplier <= 0)
            throw new BusinessException("INVALID_MULTIPLIERS", "Çarpanlar 0'dan büyük olmalıdır.");
    }
}

