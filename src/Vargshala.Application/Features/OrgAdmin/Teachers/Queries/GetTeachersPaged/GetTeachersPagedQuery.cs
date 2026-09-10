using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeachersPaged;

public record GetTeachersPagedQuery(
    PagedRequest Request,
    string? Department = null,
    string? Designation = null,
    bool? IsActive = null,
    Guid? BranchId = null
) : IRequest<ApiResponse<PagedResponse<TeacherDto>>>;
