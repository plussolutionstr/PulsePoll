using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SurveyResultScriptConfiguration : IEntityTypeConfiguration<SurveyResultScript>
{
    public void Configure(EntityTypeBuilder<SurveyResultScript> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("uq_survey_result_scripts_name");
    }
}

public class SurveyResultPatternConfiguration : IEntityTypeConfiguration<SurveyResultPattern>
{
    public void Configure(EntityTypeBuilder<SurveyResultPattern> builder)
    {
        builder.Property(x => x.MatchPattern)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.HasOne(x => x.SurveyResultScript)
            .WithMany(x => x.Patterns)
            .HasForeignKey(x => x.SurveyResultScriptId)
            .HasConstraintName("fk_survey_result_patterns_script")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SurveyResultScriptId, x.Order })
            .HasDatabaseName("idx_survey_result_patterns_script_order");
    }
}
