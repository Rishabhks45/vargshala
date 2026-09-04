using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Commands.UpdateControlPanelUser;

public record UpdateControlPanelUserCommand(UpdateUserRequest Request) : IRequest<ApiResponse<UserDto>>;
