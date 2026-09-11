using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Commands.AssignStudentFee;

public record AssignStudentFeeCommand(AssignStudentFeeRequest Request) : IRequest<ApiResponse<StudentFeeDto>>;
