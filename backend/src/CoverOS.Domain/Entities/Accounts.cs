namespace CoverOS.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public sealed class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Agency";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public sealed class OrganizationMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Role { get; set; } = "AgencyAdmin";
    public string Status { get; set; } = "Active";
}
public sealed class AgencyContractor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgencyId { get; set; }
    public Organization Agency { get; set; } = null!;
    public Guid ContractorId { get; set; }
    public Organization Contractor { get; set; } = null!;
    public Guid? AssignedAgentUserId { get; set; }
    public User? AssignedAgentUser { get; set; }
    public string Status { get; set; } = "Active";
}
