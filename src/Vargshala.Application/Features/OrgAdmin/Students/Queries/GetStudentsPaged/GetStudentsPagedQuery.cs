using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentsPaged;

public record GetStudentsPagedQuery(
    PagedRequest Request,
    string? ClassName = null,
    string? Section = null,
    bool? IsActive = null
) : IRequest<ApiResponse<PagedResponse<StudentDto>>>;
