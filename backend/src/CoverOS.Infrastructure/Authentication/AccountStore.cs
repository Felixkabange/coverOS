using CoverOS.Application.Authentication;
using CoverOS.Domain.Entities;
using CoverOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CoverOS.Infrastructure.Authentication;
public sealed class AccountStore(CoverOsDbContext db) : IAccountStore
{
    private IQueryable<OrganizationMember> Active => db.OrganizationMembers.AsNoTracking()
        .Include(x => x.User).Include(x => x.Organization)
        .Where(x => x.Status == "Active" && x.User.Status == "Active" && x.Organization.Status == "Active");
    public async Task<AccountIdentity?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var member = await Active.Where(x => x.User.Email == email).OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        return member is null ? null : new(member.User, member);
    }
    public async Task<AccountIdentity?> FindByIdAsync(Guid userId, Guid organizationId, CancellationToken ct)
    {
        var member = await Active.SingleOrDefaultAsync(x => x.UserId == userId && x.OrganizationId == organizationId, ct);
        return member is null ? null : new(member.User, member);
    }
    public async Task<bool> CreateAgencyAsync(User user, Organization organization, CancellationToken ct)
    {
        db.OrganizationMembers.Add(new OrganizationMember { User = user, Organization = organization });
        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { db.ChangeTracker.Clear(); return false; }
    }
}
