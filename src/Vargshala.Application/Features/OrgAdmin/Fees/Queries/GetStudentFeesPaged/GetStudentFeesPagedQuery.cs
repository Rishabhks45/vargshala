using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentFeesPaged;

public record GetStudentFeesPagedQuery(
    PagedRequest Request,
    Guid? BranchId = null,
    Guid? ClassId = null,
    Guid? BatchId = null,
    string? Status = null) : IRequest<ApiResponse<PagedResponse<StudentFeeDto>>>;

public class GetStudentFeesPagedQueryHandler : IRequestHandler<GetStudentFeesPagedQuery, ApiResponse<PagedResponse<StudentFeeDto>>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly ICurrentUser _currentUser;

    public GetStudentFeesPagedQueryHandler(IFeeRepository feeRepository, ICurrentUser currentUser)
    {
        _feeRepository = feeRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<StudentFeeDto>>> Handle(GetStudentFeesPagedQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<StudentFeeDto>>.FailureResponse("No active organization context found.");
        }

        var (items, totalRecords) = await _feeRepository.GetPagedStudentFeesAsync(
            orgId.Value,
            query.Request,
            query.BranchId,
            query.ClassId,
            query.BatchId,
            query.Status,
            cancellationToken);

        var dtos = items.Select(i => i.ToDto()).ToList();

        var pagedResponse = PagedResponse<StudentFeeDto>.Create(
            dtos,
            totalRecords,
            query.Request.PageNumber,
            query.Request.PageSize);

        return ApiResponse<PagedResponse<StudentFeeDto>>.SuccessResponse(pagedResponse);
    }
}

