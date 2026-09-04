using MediatR;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Organizations.Commands.ToggleStatus;

public class ToggleOrganizationStatusCommandHandler : IRequestHandler<ToggleOrganizationStatusCommand, ApiResponse<bool>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public ToggleOrganizationStatusCommandHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<ApiResponse<bool>> Handle(
        ToggleOrganizationStatusCommand request,
        CancellationToken cancellationToken)
    {
        var org = await _organizationRepository.GetByIdForUpdateAsync(request.OrganizationId, cancellationToken);
        if (org is null)
        {
            return ApiResponse<bool>.FailureResponse("Organization not found.");
        }

        org.IsActive = !org.IsActive;
        _organizationRepository.Update(org);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(org.IsActive, $"Institute status set to {(org.IsActive ? "Active" : "Suspended")}.");
    }
}
