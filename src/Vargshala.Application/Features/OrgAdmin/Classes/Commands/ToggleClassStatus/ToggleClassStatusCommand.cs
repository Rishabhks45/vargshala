using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.ToggleClassStatus;

public record ToggleClassStatusCommand(Guid Id) : IRequest<ApiResponse<bool>>;
