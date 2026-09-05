using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetAllActiveBranches;

public class GetAllActiveBranchesQueryHandler : IRequestHandler<GetAllActiveBranchesQuery, ApiResponse<List<BranchDto>>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public GetAllActiveBranchesQueryHandler(
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<BranchDto>>> Handle(
        GetAllActiveBranchesQuery query,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<BranchDto>>.FailureResponse("No active organization context found.");
        }

        var branches = await _branchRepository.GetAllActiveByOrgAsync(orgId.Value, cancellationToken);
        var dtos = branches.Select(b => b.ToDto()).ToList();

        return ApiResponse<List<BranchDto>>.SuccessResponse(dtos);
    }
}
