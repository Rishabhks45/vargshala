using MediatR;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.RegisterOrganization;

public record RegisterOrganizationCommand(
    string OrganizationName,
    string OrganizationCode,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    string? AdminMobile,
    string Password
) : IRequest<ApiResponse<LoginResponse>>;
