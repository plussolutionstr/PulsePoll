using FluentValidation;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;

namespace PulsePoll.Application.Services;

public class ProjectService(
    IProjectRepository repository,
    IMediaUrlService mediaUrlService,
    IRewardUnitConfigService rewardUnitConfigService,
    IValidator<CreateProjectDto> createValidator,
    IValidator<UpdateProjectDto> updateValidator) : IProjectService
{
    private const string MediaBucket = "media-library";

    public async Task<List<ProjectDto>> GetAllAsync()
    {
        var projects = await repository.GetAllAsync();
        var dtos = new List<ProjectDto>(projects.Count);
        foreach (var p in projects)
            dtos.Add(await ToDtoAsync(p));
        return dtos;
    }

    public async Task<ProjectDto?> GetByIdAsync(int projectId)
    {
        var project = await repository.GetByIdAsync(projectId);
        return project is null ? null : await ToDtoAsync(project);
    }

    public async Task<List<ProjectDto>> GetAssignedProjectsAsync(int subjectId)
    {
        var projects = await repository.GetAssignedToSubjectAsync(subjectId);
        var dtos = new List<ProjectDto>(projects.Count);
        foreach (var p in projects)
        {
            var assignment = p.Assignments.FirstOrDefault(a => a.SubjectId == subjectId);
            if (assignment is null || IsHiddenForSubjectList(assignment.Status))
                continue;

            dtos.Add(await ToDtoAsync(p, assignment.Status));
        }
        return dtos;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, int adminId)
    {
        await createValidator.ValidateAndThrowAsync(dto);

        if (await repository.ExistsByCodeAsync(dto.Code))
            throw new BusinessException("PROJECT_CODE_EXISTS", $"'{dto.Code}' kodlu proje zaten mevcut.");

        var project = new Project
        {
            CustomerId           = dto.CustomerId,
            Code                 = dto.Code.ToUpperInvariant(),
            Name                 = dto.Name,
            Description          = dto.Description,
            Category             = dto.Category,
            ParticipantCount     = dto.ParticipantCount,
            TotalTargetCount     = dto.TotalTargetCount,
            DurationDays         = dto.DurationDays,
            StartDate            = dto.StartDate,
            Budget               = dto.Budget,
            Reward               = dto.Reward,
            ConsolationReward    = dto.ConsolationReward,
            SurveyUrl            = dto.SurveyUrl,
            SubjectParameterName = dto.SubjectParameterName,
            ProjectParameterName = dto.ProjectParameterName,
            EstimatedMinutes     = dto.EstimatedMinutes,
            CustomerBriefing     = dto.CustomerBriefing,
            StartMessage         = dto.StartMessage,
            CompletedMessage     = dto.CompletedMessage,
            DisqualifyMessage    = dto.DisqualifyMessage,
            QuotaFullMessage     = dto.QuotaFullMessage,
            ScreenOutMessage     = dto.ScreenOutMessage,
            CoverMediaId         = dto.CoverMediaId
        };
        project.SetCreated(adminId);

        await repository.AddAsync(project);

        var created = await repository.GetByIdAsync(project.Id)
            ?? throw new NotFoundException("Proje");
        return await ToDtoAsync(created);
    }

    public async Task UpdateAsync(int id, UpdateProjectDto dto, int adminId)
    {
        await updateValidator.ValidateAndThrowAsync(dto);

        var project = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Proje");

        project.Name                 = dto.Name;
        project.Description          = dto.Description;
        project.Category             = dto.Category;
        project.ParticipantCount     = dto.ParticipantCount;
        project.TotalTargetCount     = dto.TotalTargetCount;
        project.DurationDays         = dto.DurationDays;
        project.StartDate            = dto.StartDate;
        project.Budget               = dto.Budget;
        project.Reward               = dto.Reward;
        project.ConsolationReward    = dto.ConsolationReward;
        project.SurveyUrl            = dto.SurveyUrl;
        project.SubjectParameterName = dto.SubjectParameterName;
        project.ProjectParameterName = dto.ProjectParameterName;
        project.EstimatedMinutes     = dto.EstimatedMinutes;
        project.CustomerBriefing     = dto.CustomerBriefing;
        project.StartMessage         = dto.StartMessage;
        project.CompletedMessage     = dto.CompletedMessage;
        project.DisqualifyMessage    = dto.DisqualifyMessage;
        project.QuotaFullMessage     = dto.QuotaFullMessage;
        project.ScreenOutMessage     = dto.ScreenOutMessage;
        project.Status               = dto.Status;
        project.CoverMediaId         = dto.CoverMediaId;
        project.SetUpdated(adminId);

        await repository.UpdateAsync(project);
    }

    public async Task SetStatusAsync(int id, ProjectStatus status, int adminId)
    {
        var project = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Proje");

        if (project.Status == status)
            return;

        project.Status = status;
        project.SetUpdated(adminId);
        await repository.UpdateAsync(project);
    }

    public async Task DeleteAsync(int id, int adminId)
    {
        var project = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Proje");

        project.SetDeleted(adminId);
        await repository.DeleteAsync(project);
    }

    private async Task<ProjectDto> ToDtoAsync(Project p, AssignmentStatus? assignmentStatus = null)
    {
        var rewardUnit = await rewardUnitConfigService.GetAsync();

        string? coverImageUrl = null;
        if (p.CoverMedia is not null)
            coverImageUrl = await mediaUrlService.GetMediaUrlAsync(MediaBucket, p.CoverMedia.ObjectKey);

        return new ProjectDto(
            p.Id,
            p.CustomerId,
            p.Customer?.ShortName ?? string.Empty,
            p.Code,
            p.Name,
            p.Description,
            p.Category,
            p.ParticipantCount,
            p.TotalTargetCount,
            p.DurationDays,
            p.StartDate,
            p.EndDate,
            p.Budget,
            p.Reward,
            p.ConsolationReward,
            p.SurveyUrl,
            p.SubjectParameterName,
            p.ProjectParameterName,
            p.EstimatedMinutes,
            p.CustomerBriefing,
            p.StartMessage,
            p.CompletedMessage,
            p.DisqualifyMessage,
            p.QuotaFullMessage,
            p.ScreenOutMessage,
            p.Status,
            assignmentStatus,
            p.CoverMediaId,
            coverImageUrl,
            rewardUnit.UnitCode,
            rewardUnit.UnitLabel,
            rewardUnit.TryMultiplier);
    }

    private static bool IsHiddenForSubjectList(AssignmentStatus status)
        => status is AssignmentStatus.Completed
            or AssignmentStatus.Disqualify
            or AssignmentStatus.QuotaFull
            or AssignmentStatus.ScreenOut;
}
