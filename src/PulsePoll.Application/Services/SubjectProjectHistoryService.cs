using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Application.Services;

public class SubjectProjectHistoryService(IProjectRepository projectRepository)
{
    public async Task<List<ProjectHistoryDto>> GetSubjectProjectHistoryAsync(int subjectId)
    {
        var assignments = await projectRepository.GetSubjectAssignmentsAsync(subjectId);
        return assignments.Select(a => new ProjectHistoryDto(
            a.Project.Id,
            a.Project.Name,
            a.Project.Customer.ShortName,
            a.Status,
            a.AssignedAt,
            a.CompletedAt,
            CalculateDurationMinutes(a),
            a.EarnedAmount ?? 0
        )).ToList();
    }

    private static int CalculateDurationMinutes(Domain.Entities.ProjectAssignment assignment)
    {
        if (!assignment.CompletedAt.HasValue)
            return 0;
        return (int)(assignment.CompletedAt.Value - assignment.AssignedAt).TotalMinutes;
    }
}
