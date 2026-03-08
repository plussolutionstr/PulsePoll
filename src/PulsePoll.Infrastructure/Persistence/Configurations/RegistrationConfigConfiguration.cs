using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class RegistrationConfigConfiguration : IEntityTypeConfiguration<RegistrationConfig>
{
    public const int SingletonId = 1;

    public void Configure(EntityTypeBuilder<RegistrationConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.AutoApproveNewSubjects)
            .IsRequired()
            .HasDefaultValue(true);

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_registration_configs_singleton",
            "id = 1"));
    }
}
