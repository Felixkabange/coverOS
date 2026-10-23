using CoverOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoverOS.Infrastructure.Persistence;

public sealed class CoverOsDbContext(DbContextOptions<CoverOsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<AgencyContractor> AgencyContractors => Set<AgencyContractor>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(e => {
            e.ToTable("users", t => t.HasCheckConstraint("CK_User_NormalizedEmail", "\"Email\" = lower(btrim(\"Email\"))")); e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(254).IsRequired(); e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Status).HasMaxLength(24).IsRequired();
        });
        model.Entity<Organization>(e => {
            e.ToTable("organizations", t => t.HasCheckConstraint("CK_Organization_Type", "\"Type\" IN ('Agency', 'Contractor')"));
            e.HasKey(x => x.Id); e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Type).HasMaxLength(24).IsRequired(); e.Property(x => x.Status).HasMaxLength(24).IsRequired();
        });
        model.Entity<OrganizationMember>(e => {
            e.ToTable("organization_members", t => t.HasCheckConstraint("CK_Member_Role", "\"Role\" IN ('AgencyAdmin', 'AgencyStaff', 'Contractor')"));
            e.HasKey(x => x.Id); e.HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();
            e.Property(x => x.Role).HasMaxLength(24).IsRequired(); e.Property(x => x.Status).HasMaxLength(24).IsRequired();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<AgencyContractor>(e => {
            e.ToTable("agency_contractors", t => t.HasCheckConstraint("CK_Link_DifferentOrganizations", "\"AgencyId\" <> \"ContractorId\""));
            e.HasKey(x => x.Id); e.HasIndex(x => new { x.AgencyId, x.ContractorId }).IsUnique();
            e.Property(x => x.Status).HasMaxLength(24).IsRequired();
            e.HasOne(x => x.Agency).WithMany().HasForeignKey(x => x.AgencyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Contractor).WithMany().HasForeignKey(x => x.ContractorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedAgentUser).WithMany().HasForeignKey(x => x.AssignedAgentUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
