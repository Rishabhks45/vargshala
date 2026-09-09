using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Queries.GetAllActiveClasses;

public class GetAllActiveClassesQueryHandler : IRequestHandler<GetAllActiveClassesQuery, ApiResponse<List<ClassLookupDto>>>
{
    private readonly IClassRepository _classRepository;
    private readonly ICurrentUser _currentUser;

    public GetAllActiveClassesQueryHandler(
        IClassRepository classRepository,
        ICurrentUser currentUser)
    {
        _classRepository = classRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<ClassLookupDto>>> Handle(GetAllActiveClassesQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<ClassLookupDto>>.FailureResponse("No active organization context found.");
        }

        var items = await _classRepository.GetAllActiveAsync(orgId.Value, query.BranchId, cancellationToken);
        var dtos = items.Select(c => c.ToLookupDto()).ToList();
        return ApiResponse<List<ClassLookupDto>>.SuccessResponse(dtos);
    }
}
