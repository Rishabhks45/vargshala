using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<ApiResponse<UserDto>>;
