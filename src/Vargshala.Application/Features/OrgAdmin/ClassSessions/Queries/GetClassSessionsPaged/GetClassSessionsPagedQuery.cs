using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.ClassSessions;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Queries.GetClassSessionsPaged;

public record GetClassSessionsPagedQuery(
    PagedRequest? Request,
    Guid? BranchId = null,
    Guid? ClassId = null,
    Guid? BatchId = null,
    Guid? TeacherId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? Status = null) : IRequest<ApiResponse<PagedResponse<ClassSessionDto>>>;

public class GetClassSessionsPagedQueryHandler : IRequestHandler<GetClassSessionsPagedQuery, ApiResponse<PagedResponse<ClassSessionDto>>>
{
    private readonly IClassSessionRepository _repo;
    private readonly ICurrentUser _currentUser;

    public GetClassSessionsPagedQueryHandler(
        IClassSessionRepository repo,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<ClassSessionDto>>> Handle(GetClassSessionsPagedQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<ClassSessionDto>>.FailureResponse("No active organization context found.");
        }

        var req = query.Request ?? new PagedRequest();
        var (items, totalRecords) = await _repo.GetPagedAsync(
            orgId.Value,
            req,
            query.BranchId,
            query.ClassId,
            query.BatchId,
            query.TeacherId,
            query.FromDate,
            query.ToDate,
            query.Status,
            cancellationToken);

        var dtos = items.Select(s => s.ToDto()).ToList();
        var pagedResponse = PagedResponse<ClassSessionDto>.Create(dtos, totalRecords, req.PageNumber, req.PageSize);
        return ApiResponse<PagedResponse<ClassSessionDto>>.SuccessResponse(pagedResponse);
    }
}
