using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PulsePoll.Application.Interfaces;
using PulsePoll.Domain.Entities;

namespace PulsePoll.Infrastructure.Auth;

public class JwtService(IOptions<JwtSettings> options) : IJwtService
{
    private readonly JwtSettings _settings = options.Value;
    private readonly SymmetricSecurityKey _signingKey =
        new(Encoding.UTF8.GetBytes(options.Value.SecretKey));

    public string GenerateAccessToken(Subject subject)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, subject.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, subject.Id.ToString()),
            new(ClaimTypes.Name, subject.FullName)
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshTokenValue()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var parameters = GetValidationParameters();
        parameters.ValidateLifetime = false;

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public int? GetSubjectIdFromToken(string token)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var value = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    public TokenValidationParameters GetValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _settings.Issuer,
        ValidAudience = _settings.Audience,
        IssuerSigningKey = _signingKey,
        ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewSeconds)
    };
}
