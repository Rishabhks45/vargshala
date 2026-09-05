using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetUserBranches;

public class GetUserBranchesQueryHandler : IRequestHandler<GetUserBranchesQuery, ApiResponse<List<UserBranchAccessDto>>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public GetUserBranchesQueryHandler(
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<UserBranchAccessDto>>> Handle(
        GetUserBranchesQuery query,
        CancellationToken cancellationToken)
    {
        var accesses = await _branchRepository.GetUserBranchesAsync(query.UserId, cancellationToken);
        var dtos = accesses.Select(a => a.ToDto()).ToList();

        return ApiResponse<List<UserBranchAccessDto>>.SuccessResponse(dtos);
    }
}
