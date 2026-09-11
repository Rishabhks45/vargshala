using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructureById;

public class GetFeeStructureByIdQueryHandler : IRequestHandler<GetFeeStructureByIdQuery, ApiResponse<FeeStructureDto>>
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly ICurrentUser _currentUser;

    public GetFeeStructureByIdQueryHandler(
        IFeeStructureRepository feeStructureRepository,
        ICurrentUser currentUser)
    {
        _feeStructureRepository = feeStructureRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<FeeStructureDto>> Handle(GetFeeStructureByIdQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse("No active organization context found.");
        }

        var entity = await _feeStructureRepository.GetByIdWithDetailsAsync(query.Id, cancellationToken);
        if (entity == null || entity.OrganizationId != orgId.Value)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse("Fee structure not found.");
        }

        return ApiResponse<FeeStructureDto>.SuccessResponse(entity.ToDto());
    }
}
