using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface IReportService
{
    Task<SubjectRoadmapResultDto> GetSubjectRoadmapAsync(int year);
    Task<List<int>> GetAvailableYearsAsync();
    Task<SubjectActivityResultDto> GetSubjectActivityAsync(int days);
    Task<SubjectDemographicsResultDto> GetSubjectDemographicsAsync();
}
