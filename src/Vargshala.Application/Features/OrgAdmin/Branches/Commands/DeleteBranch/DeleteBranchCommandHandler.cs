using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.DeleteBranch;

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, ApiResponse<bool>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteBranchCommandHandler(
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteBranchCommand command,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<bool>.FailureResponse("No active organization context found.");
        }

        var branch = await _branchRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<bool>.FailureResponse("Branch not found.");
        }

        if (branch.IsMainBranch)
        {
            return ApiResponse<bool>.FailureResponse("The Main Branch cannot be deleted. Designate another branch as the Main Branch first.");
        }

        branch.IsDeleted = true;
        branch.DeletedBy = _currentUser.UserId;
        branch.DeletedAt = DateTime.UtcNow;

        _branchRepository.Update(branch);
        await _branchRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Branch deleted successfully.");
    }
}
