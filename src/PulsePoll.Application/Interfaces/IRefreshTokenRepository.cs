using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllForSubjectAsync(int subjectId, string reason);
}
