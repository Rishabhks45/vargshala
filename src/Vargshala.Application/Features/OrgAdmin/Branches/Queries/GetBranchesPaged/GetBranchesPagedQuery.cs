using MediatR;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetBranchesPaged;

public record GetBranchesPagedQuery(
    PagedRequest Request,
    string? City = null,
    bool? IsActive = null) : IRequest<ApiResponse<PagedResponse<BranchDto>>>;
