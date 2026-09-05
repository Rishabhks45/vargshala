using MediatR;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetAllActiveBranches;

public record GetAllActiveBranchesQuery : IRequest<ApiResponse<List<BranchDto>>>;
