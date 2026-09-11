using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures;

public static class FeeStructureMappingExtensions
{
    public static FeeStructureDto ToDto(this FeeStructure fs)
    {
        return new FeeStructureDto
        {
            Id = fs.Id,
            OrganizationId = fs.OrganizationId,
            BranchId = fs.BranchId,
            BranchName = fs.Branch?.Name ?? string.Empty,
            ClassId = fs.ClassId,
            ClassName = fs.Class?.Name,
            Name = fs.Name,
            Description = fs.Description,
            TotalAmount = fs.TotalAmount,
            AcademicSession = fs.AcademicSession,
            Status = fs.IsActive ? EntityStatus.Active : EntityStatus.Inactive,
            CreatedAt = fs.CreatedAt,
            UpdatedAt = fs.UpdatedAt
        };
    }

    public static FeeStructureLookupDto ToLookupDto(this FeeStructure fs)
    {
        return new FeeStructureLookupDto
        {
            Id = fs.Id,
            BranchId = fs.BranchId,
            ClassId = fs.ClassId,
            Name = fs.Name,
            TotalAmount = fs.TotalAmount,
            AcademicSession = fs.AcademicSession
        };
    }
}
