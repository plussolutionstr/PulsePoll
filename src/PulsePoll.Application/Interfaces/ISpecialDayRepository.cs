using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISpecialDayRepository
{
    Task<List<SpecialDay>> GetByYearAsync(int year);
    Task<List<SpecialDay>> GetByDateAsync(DateOnly date);
    Task<SpecialDay?> GetByIdAsync(int id);
    Task<bool> ExistsByEventCodeAndDateAsync(string eventCode, DateOnly date, int? excludeId = null);
    Task ReplaceSystemYearAsync(int year, IEnumerable<SpecialDay> systemDays);
    Task AddAsync(SpecialDay day);
    Task UpdateAsync(SpecialDay day);
    Task DeleteAsync(SpecialDay day);
}
