using PulsePoll.Application.DTOs;

namespace PulsePoll.Application.Interfaces;

public interface ISubjectScoreConfigService
{
    Task<SubjectScoreConfigDto> GetAsync();
    Task SaveAsync(SubjectScoreConfigDto dto, int adminId);
}

