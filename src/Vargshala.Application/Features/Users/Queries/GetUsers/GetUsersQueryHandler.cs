using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, ApiResponse<PagedResponse<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public GetUsersQueryHandler(IUserRepository userRepository, ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<UserDto>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<PagedResponse<UserDto>>.FailureResponse(
                "No organization associated with this user.");
        }

        var paged = request.Request ?? new PagedRequest();

        var (users, totalCount) = await _userRepository.GetPagedByOrgAsync(
            _currentUser.OrganizationId.Value, paged.Page, paged.PageSize, cancellationToken);

        var dtos = users.Select(u =>
        {
            var dto = u.Adapt<UserDto>();
            dto.Role = u.Role;
            return dto;
        }).ToList();

        var response = PagedResponse<UserDto>.Create(dtos, totalCount, paged.Page, paged.PageSize);

        return ApiResponse<PagedResponse<UserDto>>.SuccessResponse(response);
    }
}
