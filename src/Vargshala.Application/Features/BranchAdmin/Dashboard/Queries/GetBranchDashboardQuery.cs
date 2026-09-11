using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.BranchAdmin.Dashboard.Infrastructure;
using Vargshala.Contracts.BranchAdmin;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.BranchAdmin.Dashboard.Queries;

public record GetBranchDashboardQuery(Guid BranchId) : IRequest<ApiResponse<BranchDashboardDto>>;

public class GetBranchDashboardQueryHandler : IRequestHandler<GetBranchDashboardQuery, ApiResponse<BranchDashboardDto>>
{
    private readonly IBranchDashboardRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetBranchDashboardQueryHandler(
        IBranchDashboardRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BranchDashboardDto>> Handle(GetBranchDashboardQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BranchDashboardDto>.FailureResponse("No active organization context found.");
        }

        var data = await _repository.GetDashboardStatsAsync(orgId.Value, request.BranchId, cancellationToken);
        if (data is null)
        {
            return ApiResponse<BranchDashboardDto>.FailureResponse("Branch not found or inactive.");
        }

        return ApiResponse<BranchDashboardDto>.SuccessResponse(data);
    }
}
