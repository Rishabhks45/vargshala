using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Users.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(Guid UserId) : IRequest<ApiResponse<bool>>;
