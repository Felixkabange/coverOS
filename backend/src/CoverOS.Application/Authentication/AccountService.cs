using System.ComponentModel.DataAnnotations;
using CoverOS.Domain.Entities;

namespace CoverOS.Application.Authentication;

public sealed record RegisterRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, StringLength(160, MinimumLength = 2)] string AgencyName,
    [Required, StringLength(128, MinimumLength = 12)] string Password);
public sealed record LoginRequest([Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(128)] string Password);
public sealed record AccountDto(Guid UserId, string Email, string Name, Guid OrganizationId,
    string OrganizationName, string OrganizationType, string Role);
public sealed record SessionDto(string AccessToken, DateTime ExpiresAt, AccountDto Account);
public sealed record AccountIdentity(User User, OrganizationMember Membership);
public interface IAccountStore
{
    Task<AccountIdentity?> FindByEmailAsync(string email, CancellationToken ct);
    Task<AccountIdentity?> FindByIdAsync(Guid userId, Guid organizationId, CancellationToken ct);
    Task<bool> CreateAgencyAsync(User user, Organization organization, CancellationToken ct);
}
public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User? user, string password);
}
public interface ISessionIssuer { SessionDto Issue(AccountDto account); }
public sealed class AccountConflictException : Exception { }
public sealed class AccountService(IAccountStore store, IPasswordService passwords, ISessionIssuer sessions)
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    public static AccountDto Describe(AccountIdentity identity) => new(identity.User.Id, identity.User.Email,
        identity.User.Name, identity.Membership.OrganizationId, identity.Membership.Organization.Name,
        identity.Membership.Organization.Type, identity.Membership.Role);
    public async Task<SessionDto> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.AgencyName))
            throw new ArgumentException("Name and agency name are required.");
        var user = new User { Email = NormalizeEmail(request.Email), Name = request.Name.Trim() };
        user.PasswordHash = passwords.Hash(user, request.Password);
        var organization = new Organization { Name = request.AgencyName.Trim() };
        if (!await store.CreateAgencyAsync(user, organization, ct)) throw new AccountConflictException();
        return sessions.Issue(new(user.Id, user.Email, user.Name, organization.Id, organization.Name, organization.Type, "AgencyAdmin"));
    }
    public async Task<SessionDto?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var identity = await store.FindByEmailAsync(NormalizeEmail(request.Email), ct);
        // Verify a dummy hash for unknown emails as well to reduce account enumeration through timing.
        var valid = passwords.Verify(identity?.User, request.Password);
        return valid && identity is not null ? sessions.Issue(Describe(identity)) : null;
    }
    public async Task<AccountDto?> GetAccountAsync(Guid userId, Guid organizationId, CancellationToken ct)
    {
        var identity = await store.FindByIdAsync(userId, organizationId, ct);
        return identity is null ? null : Describe(identity);
    }
}
