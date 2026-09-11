using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class FeeStructure : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ClassId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public string AcademicSession { get; set; } = string.Empty;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Class? Class { get; set; }
}
