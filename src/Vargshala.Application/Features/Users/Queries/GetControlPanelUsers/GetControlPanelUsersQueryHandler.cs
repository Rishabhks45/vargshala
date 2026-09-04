using Mapster;
using MediatR;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetControlPanelUsers;

public class GetControlPanelUsersQueryHandler
    : IRequestHandler<GetControlPanelUsersQuery, ApiResponse<PagedResponse<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetControlPanelUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<PagedResponse<UserDto>>> Handle(
        GetControlPanelUsersQuery request,
        CancellationToken cancellationToken)
    {
        var pagedRequest = request.Request ?? new PagedRequest();

        var (users, totalRecords) = await _userRepository.GetControlPanelUsersPagedAsync(
            pagedRequest, request.Role, request.IsActive, cancellationToken);

        var dtos = users.Select(u =>
        {
            var dto = u.Adapt<UserDto>();
            dto.Role = u.Role;
            dto.OrganizationName = u.Organization?.Name;
            dto.OrganizationCode = u.Organization?.Code;
            return dto;
        }).ToList();

        var response = PagedResponse<UserDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<UserDto>>.SuccessResponse(response);
    }
}
