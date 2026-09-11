using Vargshala.Contracts.Common;

namespace Vargshala.Contracts.FeeStructures;

public class FeeStructureDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public Guid? ClassId { get; set; }
    public string? ClassName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public string AcademicSession { get; set; } = string.Empty;
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFeeStructureRequest
{
    public Guid BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public string AcademicSession { get; set; } = string.Empty;
}

public class UpdateFeeStructureRequest
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public string AcademicSession { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class FeeStructureLookupDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string AcademicSession { get; set; } = string.Empty;
}
