using Vargshala.Contracts.Classes;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Classes;

public static class ClassMappingExtensions
{
    public static ClassDto ToDto(this Class c)
    {
        return new ClassDto
        {
            Id = c.Id,
            BranchId = c.BranchId,
            BranchName = c.Branch?.Name ?? string.Empty,
            Name = c.Name,
            Code = c.Code,
            Description = c.Description,
            IsActive = c.IsActive,
            BatchesCount = c.Batches?.Count(b => !b.IsDeleted) ?? 0,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }

    public static ClassLookupDto ToLookupDto(this Class c)
    {
        return new ClassLookupDto
        {
            Id = c.Id,
            BranchId = c.BranchId,
            Name = c.Name,
            Code = c.Code
        };
    }
}
