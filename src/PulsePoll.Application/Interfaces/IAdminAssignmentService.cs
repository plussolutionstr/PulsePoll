using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IAdminAssignmentService
{
    Task<int> RequestBulkAssignAsync(int projectId, IEnumerable<int> subjectIds, int adminId);
    Task<List<ProjectAssignmentDto>> GetAssignmentsAsync(int projectId);
    Task<List<int>> GetAssignedSubjectIdsAsync(int projectId);
    Task RemoveAssignmentAsync(int projectId, int subjectId);
    Task<RemoveAssignmentsResultDto> RemoveAssignmentsAsync(int projectId, IEnumerable<int> subjectIds, int adminId);
    Task<RewardProcessResultDto> ApproveRewardsAsync(int projectId, IEnumerable<int> subjectIds, int adminId);
    Task<RewardProcessResultDto> RejectRewardsAsync(int projectId, IEnumerable<int> subjectIds, string reason, int adminId);
    Task<List<SubjectAssignmentJobDto>> GetJobsAsync(int projectId);
    Task CancelJobAsync(int jobId);
}
