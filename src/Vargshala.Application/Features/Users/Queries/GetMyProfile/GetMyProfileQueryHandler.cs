using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Queries.GetMyProfile;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ApiResponse<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public GetMyProfileQueryHandler(IUserRepository userRepository, ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<UserDto>.FailureResponse("User not authenticated.");
        }

        var user = await _userRepository.GetByIdWithOrgAsync(_currentUser.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UserDto>.FailureResponse("User profile not found.");
        }

        var dto = new UserDto
        {
            Id = user.Id,
            OrganizationId = user.OrganizationId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Mobile = user.Mobile,
            Role = user.Role,
            EmailVerified = user.EmailVerified,
            MobileVerified = user.MobileVerified,
            IsActive = user.IsActive,
            ProfilePictureUrl = user.ProfilePictureUrl,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            OrganizationName = user.Organization?.Name,
            OrganizationCode = user.Organization?.Code
        };

        return ApiResponse<UserDto>.SuccessResponse(dto);
    }
}
