using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Branches;

public static class BranchMappingExtensions
{
    public static BranchDto ToDto(this Branch branch)
    {
        var adminAccess = branch.UserBranchAccesses?
            .FirstOrDefault(uba => uba.IsActive && uba.User != null && uba.User.Role == UserRole.BranchAdmin);
        var adminUser = adminAccess?.User;

        return new BranchDto
        {
            Id = branch.Id,
            OrganizationId = branch.OrganizationId,
            Name = branch.Name,
            Code = branch.Code,
            LogoUrl = branch.LogoUrl,
            Email = branch.Email,
            Mobile = branch.Mobile,
            AlternateMobile = branch.AlternateMobile,
            Address = branch.Address,
            City = branch.City,
            State = branch.State,
            Pincode = branch.Pincode,
            Country = branch.Country,
            IsMainBranch = branch.IsMainBranch,
            UseBranchName = branch.UseBranchName,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt,
            UpdatedAt = branch.UpdatedAt,
            BranchAdminId = adminUser?.Id,
            BranchAdminName = adminUser != null ? $"{adminUser.FirstName} {adminUser.LastName}".Trim() : null,
            BranchAdminEmail = adminUser?.Email,
            BranchAdminMobile = adminUser?.Mobile
        };
    }

    public static UserBranchAccessDto ToDto(this UserBranchAccess access)
    {
        return new UserBranchAccessDto
        {
            Id = access.Id,
            UserId = access.UserId,
            BranchId = access.BranchId,
            BranchName = access.Branch?.Name ?? string.Empty,
            BranchCode = access.Branch?.Code ?? string.Empty,
            IsActive = access.IsActive
        };
    }
}
