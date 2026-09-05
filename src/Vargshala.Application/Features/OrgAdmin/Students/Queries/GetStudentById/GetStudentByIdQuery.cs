using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentById;

public record GetStudentByIdQuery(Guid Id) : IRequest<ApiResponse<StudentDto>>;
