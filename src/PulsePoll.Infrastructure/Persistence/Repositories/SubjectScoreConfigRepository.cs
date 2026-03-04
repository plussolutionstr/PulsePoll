using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SubjectScoreConfigRepository(AppDbContext db) : ISubjectScoreConfigRepository
{
    public Task<SubjectScoreConfig?> GetCurrentAsync()
        => db.SubjectScoreConfigs.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).FirstOrDefaultAsync();

    public async Task UpsertAsync(SubjectScoreConfig config, int actorId = 0)
    {
        var current = await GetCurrentAsync();
        if (current is null)
        {
            config.SetCreated(actorId);
            db.SubjectScoreConfigs.Add(config);
        }
        else
        {
            current.ParticipationWeight = config.ParticipationWeight;
            current.CompletionWeight = config.CompletionWeight;
            current.QualityWeight = config.QualityWeight;
            current.ApprovalTrustWeight = config.ApprovalTrustWeight;
            current.SpeedWeight = config.SpeedWeight;
            current.ConfidencePivot = config.ConfidencePivot;
            current.ScoreBaseline = config.ScoreBaseline;
            current.Star1Max = config.Star1Max;
            current.Star2Max = config.Star2Max;
            current.Star3Max = config.Star3Max;
            current.Star4Max = config.Star4Max;
            current.VeryActiveLastSeenDays = config.VeryActiveLastSeenDays;
            current.ActiveLastSeenDays = config.ActiveLastSeenDays;
            current.WarmLastSeenDays = config.WarmLastSeenDays;
            current.CoolingLastSeenDays = config.CoolingLastSeenDays;
            current.VeryActiveMinDays30 = config.VeryActiveMinDays30;
            current.VeryActiveMultiplier = config.VeryActiveMultiplier;
            current.ActiveMultiplier = config.ActiveMultiplier;
            current.WarmMultiplier = config.WarmMultiplier;
            current.CoolingMultiplier = config.CoolingMultiplier;
            current.DormantMultiplier = config.DormantMultiplier;
            current.NoTelemetryMultiplier = config.NoTelemetryMultiplier;
            current.SetUpdated(actorId);
        }

        await db.SaveChangesAsync();
    }
}

