using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, ApiResponse<BranchDto>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchByIdQueryHandler(
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BranchDto>> Handle(
        GetBranchByIdQuery query,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BranchDto>.FailureResponse("No active organization context found.");
        }

        var branch = await _branchRepository.GetByIdAsync(query.Id, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<BranchDto>.FailureResponse("Branch not found.");
        }

        return ApiResponse<BranchDto>.SuccessResponse(branch.ToDto());
    }
}
