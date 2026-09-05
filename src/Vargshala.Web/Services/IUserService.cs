using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Web.Services;

public interface IUserService
{
    event Action<UserDto>? OnProfileUpdated;
    UserDto? CurrentProfile { get; }
    void NotifyProfileUpdated(UserDto profile);

    Task<ApiResponse<PagedResponse<UserDto>>> GetControlPanelUsersAsync(
        PagedRequest? request = null,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<UserDto>> CreateControlPanelUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<UserDto>> UpdateControlPanelUserAsync(
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ToggleUserStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<UserDto>> GetMyProfileAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<UserDto>> UpdateMyProfileAsync(
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
