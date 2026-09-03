using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Queries.GetOrganization;

public class GetOrganizationQueryHandler
    : IRequestHandler<GetOrganizationQuery, ApiResponse<OrganizationDto>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICurrentUser _currentUser;

    public GetOrganizationQueryHandler(IOrganizationRepository organizationRepository, ICurrentUser currentUser)
    {
        _organizationRepository = organizationRepository;
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

        var organization = await _organizationRepository.GetByIdAsync(_currentUser.OrganizationId.Value, cancellationToken);

        if (organization is null)
        {
            return ApiResponse<OrganizationDto>.FailureResponse("Organization not found.");
        }

        var dto = organization.Adapt<OrganizationDto>();

        return ApiResponse<OrganizationDto>.SuccessResponse(dto);
    }
}
