using Microsoft.EntityFrameworkCore;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token)
        => db.RefreshTokens
             .Include(r => r.Subject)
             .FirstOrDefaultAsync(r => r.Token == token);

    public async Task AddAsync(RefreshToken refreshToken)
    {
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        db.RefreshTokens.Update(refreshToken);
        await db.SaveChangesAsync();
    }

    public async Task RevokeAllForSubjectAsync(int subjectId, string reason)
    {
        var tokens = await db.RefreshTokens
            .Where(r => r.SubjectId == subjectId && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
        }

        await db.SaveChangesAsync();
    }
}
