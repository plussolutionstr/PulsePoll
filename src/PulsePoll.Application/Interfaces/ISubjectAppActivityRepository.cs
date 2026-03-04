using PulsePoll.Application.DTOs;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectAppActivityRepository
{
    Task AddAsync(SubjectAppActivity activity);
    Task<Dictionary<int, SubjectActivityStats>> GetStatsBySubjectIdsAsync(IEnumerable<int> subjectIds, DateTime sinceUtc);
}

