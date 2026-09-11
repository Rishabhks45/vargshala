using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructuresPaged;

public class GetFeeStructuresPagedQueryHandler : IRequestHandler<GetFeeStructuresPagedQuery, ApiResponse<PagedResponse<FeeStructureDto>>>
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly ICurrentUser _currentUser;

    public GetFeeStructuresPagedQueryHandler(
        IFeeStructureRepository feeStructureRepository,
        ICurrentUser currentUser)
    {
        _feeStructureRepository = feeStructureRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<FeeStructureDto>>> Handle(GetFeeStructuresPagedQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<FeeStructureDto>>.FailureResponse("No active organization context found.");
        }

        var (items, totalRecords) = await _feeStructureRepository.GetPagedByOrgAsync(
            orgId.Value,
            query.Request,
            query.BranchId,
            query.ClassId,
            query.Session,
            query.IsActive,
            cancellationToken);

        var dtos = items.Select(x => x.ToDto()).ToList();
        var response = PagedResponse<FeeStructureDto>.Create(dtos, totalRecords, query.Request.PageNumber, query.Request.PageSize);

        return ApiResponse<PagedResponse<FeeStructureDto>>.SuccessResponse(response);
    }
}
