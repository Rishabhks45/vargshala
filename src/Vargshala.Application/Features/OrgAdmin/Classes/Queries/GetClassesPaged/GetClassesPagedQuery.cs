using MediatR;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetClassesPaged;

public record GetClassesPagedQuery(
    PagedRequest? Request = null,
    Guid? BranchId = null,
    bool? IsActive = null) : IRequest<ApiResponse<PagedResponse<ClassDto>>>;
