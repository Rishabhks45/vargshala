using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentsForFeeAssignment;

public record GetStudentsForFeeAssignmentQuery(
    Guid? BranchId = null,
    Guid? ClassId = null) : IRequest<ApiResponse<List<StudentLookupForFeeDto>>>;

public class GetStudentsForFeeAssignmentQueryHandler : IRequestHandler<GetStudentsForFeeAssignmentQuery, ApiResponse<List<StudentLookupForFeeDto>>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly ICurrentUser _currentUser;

    public GetStudentsForFeeAssignmentQueryHandler(IFeeRepository feeRepository, ICurrentUser currentUser)
    {
        _feeRepository = feeRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<StudentLookupForFeeDto>>> Handle(GetStudentsForFeeAssignmentQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<StudentLookupForFeeDto>>.FailureResponse("No active organization context found.");
        }

        var students = await _feeRepository.GetStudentsForFeeAssignmentAsync(
            orgId.Value,
            query.BranchId,
            query.ClassId,
            cancellationToken);

        return ApiResponse<List<StudentLookupForFeeDto>>.SuccessResponse(students);
    }
}
