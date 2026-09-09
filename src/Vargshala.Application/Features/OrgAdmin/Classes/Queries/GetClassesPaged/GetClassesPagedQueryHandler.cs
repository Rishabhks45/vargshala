using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassesPaged;

public class GetClassesPagedQueryHandler : IRequestHandler<GetClassesPagedQuery, ApiResponse<PagedResponse<ClassDto>>>
{
    private readonly IClassRepository _classRepository;
    private readonly ICurrentUser _currentUser;

    public GetClassesPagedQueryHandler(
        IClassRepository classRepository,
        ICurrentUser currentUser)
    {
        _classRepository = classRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<ClassDto>>> Handle(GetClassesPagedQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<ClassDto>>.FailureResponse("No active organization context found.");
        }

        var pagedRequest = query.Request ?? new PagedRequest();
        var (items, totalRecords) = await _classRepository.GetPagedAsync(
            orgId.Value,
            pagedRequest,
            query.BranchId,
            query.IsActive,
            cancellationToken);

        var dtos = items.Select(c => c.ToDto()).ToList();
        var response = PagedResponse<ClassDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<ClassDto>>.SuccessResponse(response);
    }
}
