using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<List<Project>> GetAllAsync();
    Task<List<Project>> GetAssignedToSubjectAsync(int subjectId);
    Task<bool> ExistsByCodeAsync(string code);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
    Task<ProjectAssignment?> GetAssignmentAsync(int projectId, int subjectId);
    Task AddAssignmentsAsync(IEnumerable<ProjectAssignment> assignments);
    Task UpdateAssignmentAsync(ProjectAssignment assignment);
    Task<List<ProjectAssignment>> GetAssignmentsByProjectAsync(int projectId);
    Task<List<ProjectAssignment>> GetAssignmentsByProjectAndSubjectsAsync(int projectId, IEnumerable<int> subjectIds);
    Task<List<ProjectAssignment>> GetAssignmentsBySubjectIdsAsync(IEnumerable<int> subjectIds);
    Task<List<int>> GetAssignedSubjectIdsAsync(int projectId);
    Task RemoveAssignmentAsync(int projectId, int subjectId);
    Task<int> RemoveAssignmentsAsync(int projectId, IEnumerable<int> subjectIds);
    Task<List<ProjectAssignment>> GetSubjectAssignmentsAsync(int subjectId);

    // Zamana yayılı dağıtım
    Task<List<Project>> GetActiveScheduledDistributionProjectsAsync();
    Task<int> GetAssignmentCountByStatusAsync(int projectId, AssignmentStatus status);
    Task<List<ProjectAssignment>> GetScheduledAssignmentsAsync(int projectId, int take);
    Task UpdateAssignmentsStatusBatchAsync(IEnumerable<int> assignmentIds, AssignmentStatus newStatus, DateTime? scheduledNotifiedAt = null);
    Task<List<ProjectAssignment>> GetNotStartedNeedingReminderAsync(int projectId, DateOnly notifiedBefore);
    Task<int> GetTodayDistributedCountAsync(int projectId, DateOnly today);
}
