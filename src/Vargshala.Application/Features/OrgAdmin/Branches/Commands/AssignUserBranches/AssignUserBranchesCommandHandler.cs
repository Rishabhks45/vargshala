using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.AssignUserBranches;

public class AssignUserBranchesCommandHandler : IRequestHandler<AssignUserBranchesCommand, ApiResponse<bool>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public AssignUserBranchesCommandHandler(
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        AssignUserBranchesCommand command,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<bool>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        await _branchRepository.AssignUserBranchesAsync(
            req.UserId,
            req.BranchIds,
            _currentUser.UserId,
            cancellationToken);

        await _branchRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "User branch access updated successfully.");
    }
}
