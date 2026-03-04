using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectScoreSnapshotRepository
{
    Task<List<SubjectScoreSnapshot>> GetBySubjectIdsAsync(IEnumerable<int> subjectIds);
    Task UpsertManyAsync(IEnumerable<SubjectScoreSnapshot> snapshots, int actorId = 0);
}

