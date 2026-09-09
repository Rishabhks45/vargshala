using MediatR;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.ResetPassword;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<ApiResponse<ResetPasswordResponse>>;
