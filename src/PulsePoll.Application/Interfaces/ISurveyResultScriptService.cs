using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface ISurveyResultScriptService
{
    Task<List<SurveyResultScriptDto>> GetAllAsync(bool includeInactive = true);
    Task<SurveyResultScriptDto?> GetByIdAsync(int id);
    Task<SurveyResultScriptDto> SaveAsync(SaveSurveyResultScriptDto dto, int adminId);
    Task DeleteAsync(int id, int adminId);
}
