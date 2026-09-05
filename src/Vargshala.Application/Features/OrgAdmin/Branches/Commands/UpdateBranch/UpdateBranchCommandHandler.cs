using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, ApiResponse<BranchDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateBranchCommandHandler(
        IVargshalaDbContext db,
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _db = db;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BranchDto>> Handle(
        UpdateBranchCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BranchDto>.FailureResponse("No active organization context found.");
        }

        var branch = await _branchRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<BranchDto>.FailureResponse("Branch not found.");
        }

        var code = req.Code.Trim();

        // Check duplicate code
        var exists = await _branchRepository.ExistsByCodeAsync(code, req.Id, cancellationToken);
        if (exists)
        {
            return ApiResponse<BranchDto>.FailureResponse($"Another branch with code '{code}' already exists in your institute.");
        }

        // If this branch is being promoted to main branch, demote any other main branch in this organization
        if (req.IsMainBranch && !branch.IsMainBranch)
        {
            var currentMainBranches = await _db.Branches
                .Where(b => b.OrganizationId == orgId.Value && b.IsMainBranch && b.Id != req.Id && !b.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var mb in currentMainBranches)
            {
                mb.IsMainBranch = false;
                mb.UpdatedAt = DateTime.UtcNow;
                mb.UpdatedBy = _currentUser.UserId;
            }
        }

        branch.Name = req.Name.Trim();
        branch.Code = code;
        branch.LogoUrl = string.IsNullOrWhiteSpace(req.LogoUrl) ? null : req.LogoUrl.Trim();
        branch.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim().ToLowerInvariant();
        branch.Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim();
        branch.AlternateMobile = string.IsNullOrWhiteSpace(req.AlternateMobile) ? null : req.AlternateMobile.Trim();
        branch.Address = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim();
        branch.City = string.IsNullOrWhiteSpace(req.City) ? null : req.City.Trim();
        branch.State = string.IsNullOrWhiteSpace(req.State) ? null : req.State.Trim();
        branch.Pincode = string.IsNullOrWhiteSpace(req.Pincode) ? null : req.Pincode.Trim();
        branch.Country = string.IsNullOrWhiteSpace(req.Country) ? null : req.Country.Trim();
        branch.IsMainBranch = req.IsMainBranch;
        branch.UseBranchName = req.UseBranchName;
        branch.IsActive = req.IsActive;
        branch.UpdatedBy = _currentUser.UserId;
        branch.UpdatedAt = DateTime.UtcNow;

        _branchRepository.Update(branch);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<BranchDto>.SuccessResponse(branch.ToDto(), "Branch updated successfully.");
    }
}
