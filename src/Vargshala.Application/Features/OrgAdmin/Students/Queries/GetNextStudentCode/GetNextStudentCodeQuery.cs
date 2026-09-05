using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetNextStudentCode;

public record GetNextStudentCodeQuery : IRequest<ApiResponse<GeneratedStudentCodeDto>>;
