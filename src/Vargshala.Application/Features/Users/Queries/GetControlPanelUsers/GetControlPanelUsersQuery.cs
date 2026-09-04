using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetControlPanelUsers;

public record GetControlPanelUsersQuery(
    PagedRequest? Request = null,
    UserRole? Role = null,
    bool? IsActive = null) 
    : IRequest<ApiResponse<PagedResponse<UserDto>>>;
