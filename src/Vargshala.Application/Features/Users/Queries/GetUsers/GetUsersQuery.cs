using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(int Page = 1, int PageSize = 20) : IRequest<ApiResponse<PagedResponse<UserDto>>>;
