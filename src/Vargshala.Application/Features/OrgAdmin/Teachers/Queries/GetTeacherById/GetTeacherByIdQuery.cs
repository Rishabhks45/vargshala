using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeacherById;

public record GetTeacherByIdQuery(Guid Id) : IRequest<ApiResponse<TeacherDto>>;
