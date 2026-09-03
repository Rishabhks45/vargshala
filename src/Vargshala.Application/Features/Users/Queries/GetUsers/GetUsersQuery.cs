using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(PagedRequest? Request = null) : IRequest<ApiResponse<PagedResponse<UserDto>>>;
