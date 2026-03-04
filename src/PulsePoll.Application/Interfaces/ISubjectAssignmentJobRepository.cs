using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectAssignmentJobRepository
{
    Task<SubjectAssignmentJob?> GetByIdAsync(int id);
    Task<List<SubjectAssignmentJob>> GetByProjectIdAsync(int projectId, int limit = 10);
    Task<int> AddAsync(SubjectAssignmentJob job);
    Task UpdateAsync(SubjectAssignmentJob job);
}
