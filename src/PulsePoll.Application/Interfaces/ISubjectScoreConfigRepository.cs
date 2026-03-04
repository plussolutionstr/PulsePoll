using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectScoreConfigRepository
{
    Task<SubjectScoreConfig?> GetCurrentAsync();
    Task UpsertAsync(SubjectScoreConfig config, int actorId = 0);
}

