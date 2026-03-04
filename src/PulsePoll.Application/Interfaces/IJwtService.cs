using System.Security.Claims;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Subject subject);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    int? GetSubjectIdFromToken(string token);
}
