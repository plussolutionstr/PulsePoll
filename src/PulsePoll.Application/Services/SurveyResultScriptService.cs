using PulsePoll.Application.DTOs;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Services;

public class SurveyResultScriptService(ISurveyResultScriptRepository repository) : ISurveyResultScriptService
{
    public async Task<List<SurveyResultScriptDto>> GetAllAsync(bool includeInactive = true)
    {
        var rows = await repository.GetAllAsync(includeInactive);
        return rows.Select(Map).ToList();
    }

    public async Task<SurveyResultScriptDto?> GetByIdAsync(int id)
    {
        var row = await repository.GetByIdAsync(id);
        return row is null ? null : Map(row);
    }

    public async Task<SurveyResultScriptDto> SaveAsync(SaveSurveyResultScriptDto dto, int adminId)
    {
        Validate(dto);

        var normalizedName = dto.Name.Trim();
        var exists = await repository.ExistsByNameAsync(normalizedName, dto.Id);
        if (exists)
            throw new BusinessException("SURVEY_SCRIPT_NAME_EXISTS", "Aynı isimde script zaten mevcut.");

        if (dto.Id is null or <= 0)
        {
            var script = new SurveyResultScript
            {
                Name = normalizedName,
                IsActive = dto.IsActive
            };
            script.SetCreated(adminId);

            script.Patterns = dto.Patterns.Select(p => new SurveyResultPattern
            {
                Status = p.Status,
                MatchPattern = p.MatchPattern.Trim(),
                Order = p.Order
            }).ToList();

            foreach (var pattern in script.Patterns)
                pattern.SetCreated(adminId);

            await repository.AddAsync(script);
            return Map(script);
        }

        var existing = await repository.GetByIdAsync(dto.Id.Value)
            ?? throw new NotFoundException("Anket scripti");

        existing.Name = normalizedName;
        existing.IsActive = dto.IsActive;
        existing.SetUpdated(adminId);

        foreach (var oldPattern in existing.Patterns.Where(x => x.DeletedAt == null))
        {
            oldPattern.SetDeleted(adminId);
        }

        var newPatterns = dto.Patterns.Select(p => new SurveyResultPattern
        {
            SurveyResultScriptId = existing.Id,
            Status = p.Status,
            MatchPattern = p.MatchPattern.Trim(),
            Order = p.Order
        }).ToList();

        foreach (var pattern in newPatterns)
            pattern.SetCreated(adminId);

        foreach (var pattern in newPatterns)
            existing.Patterns.Add(pattern);

        await repository.UpdateAsync(existing);

        var saved = await repository.GetByIdAsync(existing.Id)
            ?? throw new NotFoundException("Anket scripti");
        return Map(saved);
    }

    public async Task DeleteAsync(int id, int adminId)
    {
        var row = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Anket scripti");

        row.SetDeleted(adminId);
        foreach (var pattern in row.Patterns.Where(x => x.DeletedAt == null))
            pattern.SetDeleted(adminId);

        await repository.DeleteAsync(row);
    }

    private static void Validate(SaveSurveyResultScriptDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessException("SURVEY_SCRIPT_NAME_REQUIRED", "Script adı zorunludur.");

        if (dto.Patterns.Count == 0)
            throw new BusinessException("SURVEY_SCRIPT_PATTERN_REQUIRED", "En az bir pattern girilmelidir.");

        if (dto.Patterns.Any(p => string.IsNullOrWhiteSpace(p.MatchPattern)))
            throw new BusinessException("SURVEY_SCRIPT_PATTERN_TEXT_REQUIRED", "Pattern metni boş olamaz.");
    }

    private static SurveyResultScriptDto Map(SurveyResultScript row)
        => new(
            row.Id,
            row.Name,
            row.IsActive,
            row.Patterns
                .Where(x => x.DeletedAt == null)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Id)
                .Select(x => new SurveyResultPatternDto(x.Id, x.Status, x.MatchPattern, x.Order))
                .ToList());
}
