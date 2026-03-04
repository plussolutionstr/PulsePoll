using Microsoft.AspNetCore.Identity;
using PulsePoll.Application.Interfaces;

namespace PulsePoll.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(string.Empty, password);

    public bool Verify(string password, string hash)
        => _hasher.VerifyHashedPassword(string.Empty, hash, password)
            != PasswordVerificationResult.Failed;
}
