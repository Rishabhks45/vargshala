using MediatR;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetBranchById;

public record GetBranchByIdQuery(Guid Id) : IRequest<ApiResponse<BranchDto>>;
