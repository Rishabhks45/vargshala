using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetBranchesPaged;

public class GetBranchesPagedQueryHandler : IRequestHandler<GetBranchesPagedQuery, ApiResponse<PagedResponse<BranchDto>>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchesPagedQueryHandler(
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<BranchDto>>> Handle(
        GetBranchesPagedQuery query,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<BranchDto>>.FailureResponse("No active organization context found.");
        }

        var (items, totalRecords) = await _branchRepository.GetPagedByOrgAsync(
            orgId.Value,
            query.Request,
            query.City,
            query.IsActive,
            cancellationToken);

        var dtos = items.Select(b => b.ToDto()).ToList();

        var pagedResponse = PagedResponse<BranchDto>.Create(
            dtos,
            query.Request.PageNumber,
            query.Request.PageSize,
            totalRecords);

        return ApiResponse<PagedResponse<BranchDto>>.SuccessResponse(pagedResponse);
    }
}
