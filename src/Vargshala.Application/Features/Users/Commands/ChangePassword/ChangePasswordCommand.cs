using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand(ChangePasswordRequest Request) : IRequest<ApiResponse<bool>>;
