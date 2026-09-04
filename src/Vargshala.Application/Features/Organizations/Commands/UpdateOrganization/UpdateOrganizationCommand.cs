using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Commands.UpdateOrganization;

public record UpdateOrganizationCommand(UpdateOrganizationRequest Request) 
    : IRequest<ApiResponse<OrganizationDto>>;
