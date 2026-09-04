using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Organizations.Commands.ToggleStatus;

public record ToggleOrganizationStatusCommand(Guid OrganizationId) : IRequest<ApiResponse<bool>>;
