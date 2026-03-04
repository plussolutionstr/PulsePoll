using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SubjectScoreConfigConfiguration : IEntityTypeConfiguration<SubjectScoreConfig>
{
    public void Configure(EntityTypeBuilder<SubjectScoreConfig> builder)
    {
        builder.Property(x => x.ParticipationWeight).HasPrecision(6, 4);
        builder.Property(x => x.CompletionWeight).HasPrecision(6, 4);
        builder.Property(x => x.QualityWeight).HasPrecision(6, 4);
        builder.Property(x => x.ApprovalTrustWeight).HasPrecision(6, 4);
        builder.Property(x => x.SpeedWeight).HasPrecision(6, 4);

        builder.Property(x => x.ScoreBaseline).HasPrecision(6, 2);
        builder.Property(x => x.Star1Max).HasPrecision(6, 2);
        builder.Property(x => x.Star2Max).HasPrecision(6, 2);
        builder.Property(x => x.Star3Max).HasPrecision(6, 2);
        builder.Property(x => x.Star4Max).HasPrecision(6, 2);

        builder.Property(x => x.VeryActiveMultiplier).HasPrecision(6, 3);
        builder.Property(x => x.ActiveMultiplier).HasPrecision(6, 3);
        builder.Property(x => x.WarmMultiplier).HasPrecision(6, 3);
        builder.Property(x => x.CoolingMultiplier).HasPrecision(6, 3);
        builder.Property(x => x.DormantMultiplier).HasPrecision(6, 3);
        builder.Property(x => x.NoTelemetryMultiplier).HasPrecision(6, 3);
    }
}

