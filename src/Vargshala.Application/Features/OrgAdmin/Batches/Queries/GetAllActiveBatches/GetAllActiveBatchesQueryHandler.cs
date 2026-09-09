using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetAllActiveBatches;

public class GetAllActiveBatchesQueryHandler : IRequestHandler<GetAllActiveBatchesQuery, ApiResponse<List<BatchDto>>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICurrentUser _currentUser;

    public GetAllActiveBatchesQueryHandler(
        IBatchRepository batchRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<BatchDto>>> Handle(GetAllActiveBatchesQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<BatchDto>>.FailureResponse("No active organization context found.");
        }

        var items = await _batchRepository.GetAllActiveAsync(orgId.Value, query.ClassId, cancellationToken);
        var dtos = items.Select(b => b.ToDto()).ToList();
        return ApiResponse<List<BatchDto>>.SuccessResponse(dtos);
    }
}
