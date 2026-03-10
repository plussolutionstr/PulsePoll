using PulsePoll.Application.DTOs;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectAppActivityRepository
{
    Task<SubjectAppActivity?> GetBySubjectAndDateAsync(int subjectId, DateOnly date, CancellationToken ct);
    Task AddAsync(SubjectAppActivity activity, CancellationToken ct);
    Task UpdateAsync(SubjectAppActivity activity, CancellationToken ct);
    Task<Dictionary<int, SubjectActivityStats>> GetStatsBySubjectIdsAsync(IEnumerable<int> subjectIds, DateTime sinceUtc);
}
