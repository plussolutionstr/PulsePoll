using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(int id)
        => db.Projects
             .Include(p => p.Customer)
             .Include(p => p.CoverMedia)
             .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

    public Task<List<Project>> GetAllAsync()
        => db.Projects
             .Include(p => p.Customer)
             .Include(p => p.CoverMedia)
             .Where(p => p.DeletedAt == null)
             .OrderByDescending(p => p.CreatedAt)
             .ToListAsync();

    public Task<List<Project>> GetAssignedToSubjectAsync(int subjectId)
        => db.ProjectAssignments
             .Where(a => a.SubjectId == subjectId)
             .Include(a => a.Project).ThenInclude(p => p.Customer)
             .Include(a => a.Project).ThenInclude(p => p.CoverMedia)
             .Include(a => a.Project).ThenInclude(p => p.Assignments)
             .Select(a => a.Project)
             .Where(p => p.DeletedAt == null)
             .ToListAsync();

    public Task<bool> ExistsByCodeAsync(string code)
        => db.Projects.AnyAsync(p => p.Code == code && p.DeletedAt == null);

    public async Task AddAsync(Project project)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        db.Projects.Update(project);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        db.Projects.Update(project);
        await db.SaveChangesAsync();
    }

    public Task<ProjectAssignment?> GetAssignmentAsync(int projectId, int subjectId)
        => db.ProjectAssignments
             .FirstOrDefaultAsync(a => a.ProjectId == projectId && a.SubjectId == subjectId);

    public async Task AddAssignmentsAsync(IEnumerable<ProjectAssignment> assignments)
    {
        db.ProjectAssignments.AddRange(assignments);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAssignmentAsync(ProjectAssignment assignment)
    {
        db.ProjectAssignments.Update(assignment);
        await db.SaveChangesAsync();
    }

    public Task<List<ProjectAssignment>> GetAssignmentsByProjectAsync(int projectId)
        => db.ProjectAssignments
             .Include(a => a.Subject).ThenInclude(s => s.City)
             .Include(a => a.Subject).ThenInclude(s => s.SocioeconomicStatus)
             .Where(a => a.ProjectId == projectId)
             .OrderByDescending(a => a.AssignedAt)
             .ToListAsync();

    public Task<List<ProjectAssignment>> GetAssignmentsByProjectAndSubjectsAsync(int projectId, IEnumerable<int> subjectIds)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            return Task.FromResult(new List<ProjectAssignment>());

        return db.ProjectAssignments
            .Where(a => a.ProjectId == projectId && ids.Contains(a.SubjectId))
            .ToListAsync();
    }

    public Task<List<ProjectAssignment>> GetAssignmentsBySubjectIdsAsync(IEnumerable<int> subjectIds)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            return Task.FromResult(new List<ProjectAssignment>());

        return db.ProjectAssignments
            .AsNoTracking()
            .Where(a => ids.Contains(a.SubjectId))
            .ToListAsync();
    }

    public Task<List<int>> GetAssignedSubjectIdsAsync(int projectId)
        => db.ProjectAssignments
             .Where(a => a.ProjectId == projectId)
             .Select(a => a.SubjectId)
             .ToListAsync();

    public async Task RemoveAssignmentAsync(int projectId, int subjectId)
    {
        var assignment = await db.ProjectAssignments
            .FirstOrDefaultAsync(a => a.ProjectId == projectId && a.SubjectId == subjectId);
        if (assignment is not null)
        {
            db.ProjectAssignments.Remove(assignment);
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> RemoveAssignmentsAsync(int projectId, IEnumerable<int> subjectIds)
    {
        var ids = subjectIds.Distinct().ToArray();
        if (ids.Length == 0)
            return 0;

        var assignments = await db.ProjectAssignments
            .Where(a => a.ProjectId == projectId && ids.Contains(a.SubjectId))
            .ToListAsync();

        if (assignments.Count == 0)
            return 0;

        db.ProjectAssignments.RemoveRange(assignments);
        await db.SaveChangesAsync();
        return assignments.Count;
    }

    public Task<List<ProjectAssignment>> GetSubjectAssignmentsAsync(int subjectId)
        => db.ProjectAssignments
             .AsNoTracking()
             .Where(a => a.SubjectId == subjectId && a.DeletedAt == null)
             .Include(a => a.Project).ThenInclude(p => p.Customer)
             .OrderByDescending(a => a.AssignedAt)
             .ToListAsync();

    // Zamana yayılı dağıtım metodları
    public Task<List<Project>> GetActiveScheduledDistributionProjectsAsync()
        => db.Projects
             .AsNoTracking()
             .Where(p => p.Status == ProjectStatus.Active
                      && p.IsScheduledDistribution
                      && p.DeletedAt == null)
             .ToListAsync();

    public Task<int> GetAssignmentCountByStatusAsync(int projectId, AssignmentStatus status)
        => db.ProjectAssignments
             .CountAsync(a => a.ProjectId == projectId
                           && a.Status == status
                           && a.DeletedAt == null);

    public Task<List<ProjectAssignment>> GetScheduledAssignmentsAsync(int projectId, int take)
        => db.ProjectAssignments
             .Where(a => a.ProjectId == projectId
                      && a.Status == AssignmentStatus.Scheduled
                      && a.DeletedAt == null)
             .OrderBy(a => a.AssignedAt)
             .Take(take)
             .ToListAsync();

    public Task<List<AssignmentStatusCountDto>> GetAssignmentStatusCountsAsync(int projectId)
        => db.ProjectAssignments
             .AsNoTracking()
             .Where(a => a.ProjectId == projectId && a.DeletedAt == null)
             .GroupBy(a => a.Status)
             .Select(g => new AssignmentStatusCountDto(g.Key, g.Count()))
             .ToListAsync();

    public async Task UpdateAssignmentsStatusBatchAsync(IEnumerable<int> assignmentIds, AssignmentStatus newStatus, DateTime? scheduledNotifiedAt = null)
    {
        var ids = assignmentIds.ToList();
        var assignments = await db.ProjectAssignments
            .Where(a => ids.Contains(a.Id) && a.DeletedAt == null)
            .ToListAsync();

        foreach (var a in assignments)
        {
            a.Status = newStatus;
            if (scheduledNotifiedAt.HasValue)
                a.ScheduledNotifiedAt = scheduledNotifiedAt;
        }

        await db.SaveChangesAsync();
    }

    public Task<List<ProjectAssignment>> GetNotStartedNeedingReminderAsync(int projectId, DateOnly notifiedBefore)
    {
        var notifiedBeforeUtc = notifiedBefore.ToDateTime(TimeOnly.MinValue);
        return db.ProjectAssignments
            .Where(a => a.ProjectId == projectId
                     && a.Status == AssignmentStatus.NotStarted
                     && a.DeletedAt == null
                     && a.ScheduledNotifiedAt != null
                     && a.ScheduledNotifiedAt < notifiedBeforeUtc)
            .ToListAsync();
    }

    // Bildirim dağıtımı (non-scheduled projeler)
    public Task<List<Project>> GetActiveNonScheduledProjectsAsync()
        => db.Projects
             .AsNoTracking()
             .Where(p => p.Status == ProjectStatus.Active
                      && !p.IsScheduledDistribution
                      && p.DeletedAt == null)
             .ToListAsync();

    public Task<List<ProjectAssignment>> GetUnnotifiedAssignmentsAsync(int projectId, int take)
        => db.ProjectAssignments
             .Where(a => a.ProjectId == projectId
                      && a.Status == AssignmentStatus.NotStarted
                      && a.ScheduledNotifiedAt == null
                      && a.DeletedAt == null)
             .OrderBy(a => a.AssignedAt)
             .Take(take)
             .ToListAsync();
}
