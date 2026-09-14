using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetStudentFeeDetails;

public record GetStudentFeeDetailsQuery(Guid Id) : IRequest<ApiResponse<StudentFeeDetailDto>>;

public class GetStudentFeeDetailsQueryHandler : IRequestHandler<GetStudentFeeDetailsQuery, ApiResponse<StudentFeeDetailDto>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly ICurrentUser _currentUser;

    public GetStudentFeeDetailsQueryHandler(IFeeRepository feeRepository, ICurrentUser currentUser)
    {
        _feeRepository = feeRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<StudentFeeDetailDto>> Handle(GetStudentFeeDetailsQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<StudentFeeDetailDto>.FailureResponse("No active organization context found.");
        }

        var fee = await _feeRepository.GetStudentFeeDetailByIdAsync(query.Id, cancellationToken);
        if (fee == null || fee.OrganizationId != orgId.Value)
        {
            return ApiResponse<StudentFeeDetailDto>.FailureResponse("Student fee record not found.");
        }

        var payments = await _feeRepository.GetPaymentsByStudentFeeIdAsync(query.Id, cancellationToken);
        return ApiResponse<StudentFeeDetailDto>.SuccessResponse(fee.ToDetailDto(payments));
    }
}
