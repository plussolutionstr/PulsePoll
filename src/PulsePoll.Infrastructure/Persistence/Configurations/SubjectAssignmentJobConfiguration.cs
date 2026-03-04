using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Configurations;

public class SubjectAssignmentJobConfiguration : IEntityTypeConfiguration<SubjectAssignmentJob>
{
    public void Configure(EntityTypeBuilder<SubjectAssignmentJob> builder)
    {
        builder.ToTable("subject_assignment_jobs");

        builder.HasKey(j => j.Id);

        builder.HasOne(j => j.Project)
               .WithMany()
               .HasForeignKey(j => j.ProjectId)
               .HasConstraintName("fk_subject_assignment_jobs_project")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
