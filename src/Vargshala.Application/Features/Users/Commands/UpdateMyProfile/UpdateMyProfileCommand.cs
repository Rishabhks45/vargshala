using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Commands.UpdateMyProfile;

public record UpdateMyProfileCommand(UpdateMyProfileRequest Request) : IRequest<ApiResponse<UserDto>>;
