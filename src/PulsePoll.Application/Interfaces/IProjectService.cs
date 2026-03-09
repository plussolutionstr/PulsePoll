using PulsePoll.Application.DTOs;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAssignedProjectsAsync(int subjectId);
    Task<ProjectDto?> GetByIdAsync(int projectId);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto, int adminId);
    Task UpdateAsync(int id, UpdateProjectDto dto, int adminId);
    Task SetStatusAsync(int id, ProjectStatus status, int adminId);
    Task DeleteAsync(int id, int adminId);
    Task<List<ProjectDto>> GetAllAsync();
    Task<int> GetScheduledAssignmentCountAsync(int projectId);
    Task<int> UpdateAndDisableScheduledDistributionAsync(int id, UpdateProjectDto dto, int adminId);
    Task<List<ProjectSurveyHelperEntryDto>> GetSurveyHelperEntriesAsync(int projectId);
    Task<ProjectSurveyHelperEntryDto> SaveSurveyHelperEntryAsync(int projectId, SaveProjectSurveyHelperEntryDto dto, int adminId);
    Task DeleteSurveyHelperEntryAsync(int projectId, int entryId, int adminId);
    Task<ProjectSurveyHelperMatchDto> FindSurveyHelperMatchAsync(int projectId, string questionText);
}
