using PulsePoll.Application.DTOs;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Application.Services;

public class SubjectProjectHistoryService(
    IProjectRepository projectRepository,
    IRewardUnitConfigService rewardUnitConfigService)
{
    public async Task<List<ProjectHistoryDto>> GetSubjectProjectHistoryAsync(int subjectId)
    {
        var assignments = await projectRepository.GetSubjectAssignmentsAsync(subjectId);
        var rewardUnit = await rewardUnitConfigService.GetAsync();

        return assignments.Select(a => new ProjectHistoryDto(
            a.Project.Id,
            a.Project.Name,
            a.Project.Customer.ShortName,
            a.Status,
            a.RewardStatus,
            a.AssignedAt,
            a.CompletedAt,
            a.Project.EstimatedMinutes,
            a.EarnedAmount ?? 0,
            a.Project.Reward,
            a.Project.ConsolationReward,
            rewardUnit.UnitLabel
        )).ToList();
    }
}
