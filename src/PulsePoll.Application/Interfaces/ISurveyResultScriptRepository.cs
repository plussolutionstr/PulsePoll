using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISurveyResultScriptRepository
{
    Task<List<SurveyResultScript>> GetAllAsync(bool includeInactive = true);
    Task<SurveyResultScript?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task AddAsync(SurveyResultScript script);
    Task UpdateAsync(SurveyResultScript script);
    Task DeleteAsync(SurveyResultScript script);
}
