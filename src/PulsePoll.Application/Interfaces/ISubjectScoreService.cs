using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectScoreService
{
    Task<SubjectScoreDto?> GetCurrentAsync(int subjectId);
    Task<Dictionary<int, SubjectScoreDto>> GetCurrentBulkAsync(IEnumerable<int> subjectIds);
    Task RecalculateAsync(int subjectId);
    Task RecalculateAllAsync();
}

