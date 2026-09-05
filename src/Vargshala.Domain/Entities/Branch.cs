using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Branch : BaseEntity
{
    public Guid OrganizationId { get; set; }

    // Branch Details
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? AlternateMobile { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }

    // Branch Settings
    public bool IsMainBranch { get; set; } = false;
    public bool UseBranchName { get; set; } = true;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public ICollection<UserBranchAccess> UserBranchAccesses { get; set; } = new List<UserBranchAccess>();
}
