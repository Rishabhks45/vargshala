using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetAllActiveFeeStructures;

public class GetAllActiveFeeStructuresQueryHandler : IRequestHandler<GetAllActiveFeeStructuresQuery, ApiResponse<List<FeeStructureLookupDto>>>
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly ICurrentUser _currentUser;

    public GetAllActiveFeeStructuresQueryHandler(
        IFeeStructureRepository feeStructureRepository,
        ICurrentUser currentUser)
    {
        _feeStructureRepository = feeStructureRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<FeeStructureLookupDto>>> Handle(GetAllActiveFeeStructuresQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<FeeStructureLookupDto>>.FailureResponse("No active organization context found.");
        }

        var items = await _feeStructureRepository.GetAllActiveAsync(
            orgId.Value,
            query.BranchId,
            query.ClassId,
            cancellationToken);

        var dtos = items.Select(x => x.ToLookupDto()).ToList();
        return ApiResponse<List<FeeStructureLookupDto>>.SuccessResponse(dtos);
    }
}
