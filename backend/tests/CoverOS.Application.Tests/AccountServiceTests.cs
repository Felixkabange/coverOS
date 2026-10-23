using CoverOS.Application.Authentication;
using CoverOS.Domain.Entities;

namespace CoverOS.Application.Tests;
public sealed class AccountServiceTests
{
    private sealed class Store : IAccountStore
    {
        public AccountIdentity? Identity;
        public bool Duplicate;
        public Task<AccountIdentity?> FindByEmailAsync(string email, CancellationToken ct) => Task.FromResult(Identity?.User.Email == email ? Identity : null);
        public Task<AccountIdentity?> FindByIdAsync(Guid userId, Guid organizationId, CancellationToken ct) => Task.FromResult(Identity?.User.Id == userId && Identity.Membership.OrganizationId == organizationId ? Identity : null);
        public Task<bool> CreateAgencyAsync(User user, Organization organization, CancellationToken ct)
        {
            if (Duplicate) return Task.FromResult(false);
            Identity = new(user, new OrganizationMember { User = user, UserId = user.Id, Organization = organization, OrganizationId = organization.Id });
            return Task.FromResult(true);
        }
    }
    private sealed class Passwords : IPasswordService
    {
        public string Hash(User user, string password) => "test-hash:" + password;
        public bool Verify(User? user, string password) => user is not null && user.PasswordHash == Hash(user, password);
    }
    private sealed class Sessions : ISessionIssuer
    {
        public SessionDto Issue(AccountDto account) => new("test-token", DateTime.UtcNow.AddMinutes(30), account);
    }
    [Fact]
    public async Task RegistrationCreatesAgencyAdminAndNormalizesEmail()
    {
        var store = new Store(); var service = new AccountService(store, new Passwords(), new Sessions());
        var result = await service.RegisterAsync(new(" SARAH@EXAMPLE.COM ", " Sarah ", " ABC Insurance ", "long-password"), default);
        Assert.Equal("sarah@example.com", result.Account.Email);
        Assert.Equal("ABC Insurance", result.Account.OrganizationName);
        Assert.Equal("AgencyAdmin", result.Account.Role);
        Assert.Equal("Agency", result.Account.OrganizationType);
        Assert.NotEqual("long-password", store.Identity!.User.PasswordHash);
    }
    [Fact]
    public async Task DuplicateRegistrationDoesNotIssueSession()
    {
        var service = new AccountService(new Store { Duplicate = true }, new Passwords(), new Sessions());
        await Assert.ThrowsAsync<AccountConflictException>(() => service.RegisterAsync(new("sarah@example.com", "Sarah", "ABC Insurance", "long-password"), default));
    }
    [Fact]
    public async Task LoginRejectsUnknownEmailAndIncorrectPassword()
    {
        var service = new AccountService(new Store(), new Passwords(), new Sessions());
        await service.RegisterAsync(new("sarah@example.com", "Sarah", "ABC Insurance", "long-password"), default);
        Assert.Null(await service.LoginAsync(new("other@example.com", "long-password"), default));
        Assert.Null(await service.LoginAsync(new("sarah@example.com", "wrong-password"), default));
        Assert.NotNull(await service.LoginAsync(new("SARAH@example.com", "long-password"), default));
    }
    [Fact]
    public async Task AccountCannotBeReadFromDifferentOrganization()
    {
        var service = new AccountService(new Store(), new Passwords(), new Sessions());
        var result = await service.RegisterAsync(new("sarah@example.com", "Sarah", "ABC Insurance", "long-password"), default);
        Assert.Null(await service.GetAccountAsync(result.Account.UserId, Guid.NewGuid(), default));
        Assert.NotNull(await service.GetAccountAsync(result.Account.UserId, result.Account.OrganizationId, default));
    }
}
