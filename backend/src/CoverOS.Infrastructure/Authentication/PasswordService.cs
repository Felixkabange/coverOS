using CoverOS.Application.Authentication;
using CoverOS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CoverOS.Infrastructure.Authentication;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> hasher = new();
    private readonly User dummy = new();

    public PasswordService()
    {
        // Compute once per singleton, so unknown accounts still perform one password verification.
        dummy.PasswordHash = hasher.HashPassword(dummy, Guid.NewGuid().ToString());
    }

    public string Hash(User user, string password) => hasher.HashPassword(user, password);

    public bool Verify(User? user, string password)
    {
        var target = user ?? dummy;
        var result = hasher.VerifyHashedPassword(target, target.PasswordHash, password);
        return user is not null && result != PasswordVerificationResult.Failed;
    }
}
