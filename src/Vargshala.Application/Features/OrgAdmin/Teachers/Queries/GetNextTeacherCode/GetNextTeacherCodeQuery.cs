using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetNextTeacherCode;

public record GetNextTeacherCodeQuery : IRequest<ApiResponse<GeneratedTeacherCodeDto>>;
