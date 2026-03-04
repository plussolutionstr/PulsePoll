using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.SurveyUrl).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.SubjectParameterName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.CustomerBriefing).HasMaxLength(2000);
        builder.Property(p => p.StartMessage).IsRequired().HasMaxLength(500);
        builder.Property(p => p.CompletedMessage).IsRequired().HasMaxLength(500);
        builder.Property(p => p.DisqualifyMessage).IsRequired().HasMaxLength(500);
        builder.Property(p => p.QuotaFullMessage).IsRequired().HasMaxLength(500);
        builder.Property(p => p.ScreenOutMessage).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Budget).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Reward).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ConsolationReward).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Customer)
               .WithMany()
               .HasForeignKey(p => p.CustomerId)
               .HasConstraintName("fk_projects_customers")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CoverMedia)
               .WithMany(m => m.Projects)
               .HasForeignKey(p => p.CoverMediaId)
               .HasConstraintName("fk_projects_cover_media")
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.Code).IsUnique().HasDatabaseName("uq_projects_code");
        builder.HasIndex(p => p.CustomerId).HasDatabaseName("idx_projects_customer_id");
        builder.HasIndex(p => p.Status).HasDatabaseName("idx_projects_status");
    }
}

public class ProjectAssignmentConfiguration : IEntityTypeConfiguration<ProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
    {
        builder.Property(a => a.EarnedAmount).HasColumnType("decimal(18,2)");
        builder.Property(a => a.RewardRejectionReason).HasMaxLength(500);

        builder.HasOne(a => a.Project)
               .WithMany(p => p.Assignments)
               .HasForeignKey(a => a.ProjectId)
               .HasConstraintName("fk_project_assignments_projects")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Subject)
               .WithMany()
               .HasForeignKey(a => a.SubjectId)
               .HasConstraintName("fk_project_assignments_subjects")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.ProjectId, a.SubjectId })
               .IsUnique()
               .HasDatabaseName("uq_project_assignments_project_subject");

        builder.HasIndex(a => a.SubjectId).HasDatabaseName("idx_project_assignments_subject_id");
    }
}
