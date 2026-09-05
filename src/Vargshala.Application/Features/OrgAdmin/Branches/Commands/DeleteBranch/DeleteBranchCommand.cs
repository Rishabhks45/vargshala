using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Commands.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest<ApiResponse<bool>>;
