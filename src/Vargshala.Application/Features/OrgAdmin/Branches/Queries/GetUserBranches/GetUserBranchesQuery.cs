using MediatR;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetUserBranches;

public record GetUserBranchesQuery(Guid UserId) : IRequest<ApiResponse<List<UserBranchAccessDto>>>;
