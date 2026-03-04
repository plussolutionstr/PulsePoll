using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class SubjectAssignmentJobRepository(AppDbContext db) : ISubjectAssignmentJobRepository
{
    public Task<SubjectAssignmentJob?> GetByIdAsync(int id)
        => db.SubjectAssignmentJobs.FirstOrDefaultAsync(j => j.Id == id);

    public Task<List<SubjectAssignmentJob>> GetByProjectIdAsync(int projectId, int limit = 10)
        => db.SubjectAssignmentJobs
             .AsNoTracking()
             .Where(j => j.ProjectId == projectId)
             .OrderByDescending(j => j.CreatedAt)
             .Take(limit)
             .ToListAsync();

    public async Task<int> AddAsync(SubjectAssignmentJob job)
    {
        db.SubjectAssignmentJobs.Add(job);
        await db.SaveChangesAsync();
        return job.Id;
    }

    public async Task UpdateAsync(SubjectAssignmentJob job)
    {
        db.SubjectAssignmentJobs.Update(job);
        await db.SaveChangesAsync();
    }
}
