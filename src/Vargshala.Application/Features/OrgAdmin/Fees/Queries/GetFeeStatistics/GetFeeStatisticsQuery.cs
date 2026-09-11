using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetFeeStatistics;

public record GetFeeStatisticsQuery(Guid? BranchId = null) : IRequest<ApiResponse<FeeStatisticsDto>>;

public class GetFeeStatisticsQueryHandler : IRequestHandler<GetFeeStatisticsQuery, ApiResponse<FeeStatisticsDto>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly ICurrentUser _currentUser;

    public GetFeeStatisticsQueryHandler(IFeeRepository feeRepository, ICurrentUser currentUser)
    {
        _feeRepository = feeRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<FeeStatisticsDto>> Handle(GetFeeStatisticsQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<FeeStatisticsDto>.FailureResponse("No active organization context found.");
        }

        var stats = await _feeRepository.GetFeeStatisticsAsync(orgId.Value, query.BranchId, cancellationToken);
        return ApiResponse<FeeStatisticsDto>.SuccessResponse(stats);
    }
}
