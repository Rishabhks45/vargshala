using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Queries.GetOrganization;

public class GetOrganizationQueryHandler
    : IRequestHandler<GetOrganizationQuery, ApiResponse<OrganizationDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetOrganizationQueryHandler(IVargshalaDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<OrganizationDto>> Handle(
        GetOrganizationQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<OrganizationDto>.FailureResponse("No organization associated with this user.");
        }

        var organization = await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId && !o.IsDeleted, cancellationToken);

        if (organization is null)
        {
            return ApiResponse<OrganizationDto>.FailureResponse("Organization not found.");
        }

        var dto = organization.Adapt<OrganizationDto>();

        return ApiResponse<OrganizationDto>.SuccessResponse(dto);
    }
}
