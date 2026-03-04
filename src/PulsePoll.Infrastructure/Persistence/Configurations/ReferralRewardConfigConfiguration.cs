using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class ReferralRewardConfigConfiguration : IEntityTypeConfiguration<ReferralRewardConfig>
{
    public void Configure(EntityTypeBuilder<ReferralRewardConfig> builder)
    {
        builder.Property(x => x.RewardAmount).HasPrecision(18, 2);
        builder.Property(x => x.TriggerType).HasConversion<int>();
    }
}
