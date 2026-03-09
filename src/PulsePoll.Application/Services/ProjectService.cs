using FluentValidation;
using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;
using PulsePoll.Domain.Enums;
using System.Globalization;
using System.Text;

namespace PulsePoll.Application.Services;

public class ProjectService(
    IProjectRepository repository,
    IMediaUrlService mediaUrlService,
    IRewardUnitConfigService rewardUnitConfigService,
    IValidator<CreateProjectDto> createValidator,
    IValidator<UpdateProjectDto> updateValidator) : IProjectService
{
    private const string MediaBucket = "media-library";
    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

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
            CoverMediaId              = dto.CoverMediaId,
            IsScheduledDistribution   = dto.IsScheduledDistribution,
            DistributionStartHour     = dto.DistributionStartHour == default ? new TimeOnly(9, 0) : dto.DistributionStartHour,
            DistributionEndHour       = dto.DistributionEndHour == default ? new TimeOnly(19, 0) : dto.DistributionEndHour,
            SupportsSurveyHelper      = dto.SupportsSurveyHelper
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
        project.Status                  = dto.Status;
        project.CoverMediaId            = dto.CoverMediaId;
        project.IsScheduledDistribution = dto.IsScheduledDistribution;
        project.DistributionStartHour   = dto.DistributionStartHour == default ? new TimeOnly(9, 0) : dto.DistributionStartHour;
        project.DistributionEndHour     = dto.DistributionEndHour == default ? new TimeOnly(19, 0) : dto.DistributionEndHour;
        project.SupportsSurveyHelper    = dto.SupportsSurveyHelper;
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
            p.IsScheduledDistribution,
            p.DistributionStartHour,
            p.DistributionEndHour,
            p.SupportsSurveyHelper,
            rewardUnit.UnitCode,
            rewardUnit.UnitLabel,
            rewardUnit.TryMultiplier);
    }

    public Task<int> GetScheduledAssignmentCountAsync(int projectId)
        => repository.GetAssignmentCountByStatusAsync(projectId, AssignmentStatus.Scheduled);

    public async Task<int> UpdateAndDisableScheduledDistributionAsync(int id, UpdateProjectDto dto, int adminId)
    {
        await updateValidator.ValidateAndThrowAsync(dto);

        var project = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Proje");

        // Scheduled atamaları çek (henüz SaveChanges olmadı)
        var assignments = await repository.GetScheduledAssignmentsAsync(id, int.MaxValue);

        // Proje alanlarını güncelle
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
        project.Status                  = dto.Status;
        project.CoverMediaId            = dto.CoverMediaId;
        project.IsScheduledDistribution = false;
        project.DistributionStartHour   = dto.DistributionStartHour == default ? new TimeOnly(9, 0) : dto.DistributionStartHour;
        project.DistributionEndHour     = dto.DistributionEndHour == default ? new TimeOnly(19, 0) : dto.DistributionEndHour;
        project.SupportsSurveyHelper    = dto.SupportsSurveyHelper;
        project.SetUpdated(adminId);

        // Scheduled → NotStarted (aynı change tracker, tek SaveChanges)
        foreach (var a in assignments)
            a.Status = AssignmentStatus.NotStarted;

        await repository.UpdateAsync(project);
        return assignments.Count;
    }

    private static bool IsHiddenForSubjectList(AssignmentStatus status)
        => status is AssignmentStatus.Completed
            or AssignmentStatus.Disqualify
            or AssignmentStatus.QuotaFull
            or AssignmentStatus.ScreenOut
            or AssignmentStatus.Scheduled;

    public async Task<List<ProjectSurveyHelperEntryDto>> GetSurveyHelperEntriesAsync(int projectId)
    {
        _ = await repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        var rows = await repository.GetSurveyHelperEntriesAsync(projectId);
        return rows.Select(MapSurveyHelperEntry).ToList();
    }

    public async Task<ProjectSurveyHelperEntryDto> SaveSurveyHelperEntryAsync(int projectId, SaveProjectSurveyHelperEntryDto dto, int adminId)
    {
        var project = await repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        if (string.IsNullOrWhiteSpace(dto.QuestionText))
            throw new BusinessException("SURVEY_HELPER_QUESTION_REQUIRED", "Soru metni zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.HelpText))
            throw new BusinessException("SURVEY_HELPER_HELP_REQUIRED", "Yardım metni zorunludur.");

        if (dto.QuestionText.Trim().Length > 1000)
            throw new BusinessException("SURVEY_HELPER_QUESTION_TOO_LONG", "Soru metni 1000 karakterden uzun olamaz.");

        if (dto.HelpText.Trim().Length > 2000)
            throw new BusinessException("SURVEY_HELPER_HELP_TOO_LONG", "Yardım metni 2000 karakterden uzun olamaz.");

        ProjectSurveyHelperEntry entry;
        if (dto.Id.HasValue)
        {
            entry = await repository.GetSurveyHelperEntryByIdAsync(projectId, dto.Id.Value)
                ?? throw new NotFoundException("Survey helper kaydı");

            entry.QuestionText = dto.QuestionText.Trim();
            entry.HelpText = dto.HelpText.Trim();
            entry.SetUpdated(adminId);
            await repository.UpdateSurveyHelperEntryAsync(entry);
        }
        else
        {
            entry = new ProjectSurveyHelperEntry
            {
                ProjectId = project.Id,
                QuestionText = dto.QuestionText.Trim(),
                HelpText = dto.HelpText.Trim()
            };
            entry.SetCreated(adminId);
            await repository.AddSurveyHelperEntryAsync(entry);
        }

        return MapSurveyHelperEntry(entry);
    }

    public async Task DeleteSurveyHelperEntryAsync(int projectId, int entryId, int adminId)
    {
        _ = await repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        var entry = await repository.GetSurveyHelperEntryByIdAsync(projectId, entryId)
            ?? throw new NotFoundException("Survey helper kaydı");

        entry.SetDeleted(adminId);
        await repository.UpdateSurveyHelperEntryAsync(entry);
    }

    public async Task<ProjectSurveyHelperMatchDto> FindSurveyHelperMatchAsync(int projectId, string questionText)
    {
        var project = await repository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje");

        if (!project.SupportsSurveyHelper)
            return new ProjectSurveyHelperMatchDto(false, "Bu proje için survey helper aktif değil.");

        var normalizedQuestion = Normalize(questionText);
        if (string.IsNullOrWhiteSpace(normalizedQuestion))
            return new ProjectSurveyHelperMatchDto(false, "Soru algılanamadı.");

        var entries = await repository.GetSurveyHelperEntriesAsync(projectId);
        var match = entries
            .Select(x => new { Entry = x, NormalizedQuestion = Normalize(x.QuestionText) })
            .Where(x => !string.IsNullOrWhiteSpace(x.NormalizedQuestion))
            .OrderByDescending(x => x.NormalizedQuestion.Length)
            .FirstOrDefault(x => normalizedQuestion.Contains(x.NormalizedQuestion, StringComparison.Ordinal));

        return match is null
            ? new ProjectSurveyHelperMatchDto(false, "Bu soru için yardım bulunamadı.")
            : new ProjectSurveyHelperMatchDto(true, match.Entry.HelpText);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var lowered = value
            .Trim()
            .Replace('’', '\'')
            .Replace('‘', '\'')
            .ToLower(TrCulture);

        var sb = new StringBuilder(lowered.Length);
        var lastWasSpace = false;
        foreach (var ch in lowered)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastWasSpace = false;
                continue;
            }

            if (lastWasSpace)
                continue;

            sb.Append(' ');
            lastWasSpace = true;
        }

        return sb.ToString().Trim();
    }

    private static ProjectSurveyHelperEntryDto MapSurveyHelperEntry(ProjectSurveyHelperEntry entry)
        => new(
            entry.Id,
            entry.ProjectId,
            entry.QuestionText,
            entry.HelpText,
            entry.CreatedAt);
}
