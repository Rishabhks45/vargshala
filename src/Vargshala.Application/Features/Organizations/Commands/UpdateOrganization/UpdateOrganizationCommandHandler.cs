using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, ApiResponse<OrganizationDto>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        ICurrentUser currentUser)
    {
        _organizationRepository = organizationRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<OrganizationDto>> Handle(
        UpdateOrganizationCommand command, 
        CancellationToken cancellationToken)
    {
        var req = command.Request;

        if (!_currentUser.IsSuperAdmin && _currentUser.OrganizationId != req.Id)
        {
            return ApiResponse<OrganizationDto>.FailureResponse("Unauthorized to update this organization.");
        }

        var org = await _organizationRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);

        if (org == null)
        {
            return ApiResponse<OrganizationDto>.FailureResponse("Institute not found.");
        }

        org.Name = req.Name.Trim();
        org.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        org.Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim();
        org.Address = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim();
        org.City = string.IsNullOrWhiteSpace(req.City) ? null : req.City.Trim();
        org.State = string.IsNullOrWhiteSpace(req.State) ? null : req.State.Trim();
        org.Pincode = string.IsNullOrWhiteSpace(req.Pincode) ? null : req.Pincode.Trim();
        org.AcademicSession = string.IsNullOrWhiteSpace(req.AcademicSession) ? null : req.AcademicSession.Trim();
        org.LogoUrl = string.IsNullOrWhiteSpace(req.LogoUrl) ? null : req.LogoUrl.Trim();
        org.IsActive = req.IsActive;
        org.UpdatedAt = DateTime.UtcNow;

        _organizationRepository.Update(org);
        await _organizationRepository.SaveChangesAsync(cancellationToken);

        var dto = new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            Code = org.Code,
            Email = org.Email,
            Mobile = org.Mobile,
            Address = org.Address,
            City = org.City,
            State = org.State,
            Pincode = org.Pincode,
            AcademicSession = org.AcademicSession,
            LogoUrl = org.LogoUrl,
            IsActive = org.IsActive,
            CreatedAt = org.CreatedAt,
            UpdatedAt = org.UpdatedAt
        };

        return ApiResponse<OrganizationDto>.SuccessResponse(dto, "Institute updated successfully.");
    }
}
