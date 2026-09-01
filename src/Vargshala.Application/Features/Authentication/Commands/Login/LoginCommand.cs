using MediatR;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<ApiResponse<LoginResponse>>;
