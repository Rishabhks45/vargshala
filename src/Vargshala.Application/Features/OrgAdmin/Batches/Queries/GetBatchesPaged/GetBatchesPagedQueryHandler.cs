using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Queries.GetBatchesPaged;

public class GetBatchesPagedQueryHandler : IRequestHandler<GetBatchesPagedQuery, ApiResponse<PagedResponse<BatchDto>>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICurrentUser _currentUser;

    public GetBatchesPagedQueryHandler(
        IBatchRepository batchRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<BatchDto>>> Handle(GetBatchesPagedQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<BatchDto>>.FailureResponse("No active organization context found.");
        }

        var pagedRequest = query.Request ?? new PagedRequest();
        var (items, totalRecords) = await _batchRepository.GetPagedAsync(
            orgId.Value,
            pagedRequest,
            query.BranchId,
            query.ClassId,
            query.SubjectId,
            query.IsActive,
            cancellationToken);

        var dtos = items.Select(b => b.ToDto()).ToList();
        var response = PagedResponse<BatchDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<BatchDto>>.SuccessResponse(response);
    }
}
