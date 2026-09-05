using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, ApiResponse<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateMyProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<UserDto>> Handle(
        UpdateMyProfileCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<UserDto>.FailureResponse("User not authenticated.");
        }

        var req = command.Request;
        var user = await _userRepository.GetByIdForUpdateAsync(_currentUser.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UserDto>.FailureResponse("User profile not found.");
        }

        user.FirstName = req.FirstName.Trim();
        user.LastName = req.LastName.Trim();
        user.Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim();

        if (req.ProfilePictureUrl != null)
        {
            user.ProfilePictureUrl = string.IsNullOrWhiteSpace(req.ProfilePictureUrl) ? null : req.ProfilePictureUrl;
        }

        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Fetch fresh with organization for full DTO mapping
        var freshUser = await _userRepository.GetByIdWithOrgAsync(user.Id, cancellationToken) ?? user;

        var dto = new UserDto
        {
            Id = freshUser.Id,
            OrganizationId = freshUser.OrganizationId,
            FirstName = freshUser.FirstName,
            LastName = freshUser.LastName,
            Email = freshUser.Email,
            Mobile = freshUser.Mobile,
            Role = freshUser.Role,
            EmailVerified = freshUser.EmailVerified,
            MobileVerified = freshUser.MobileVerified,
            IsActive = freshUser.IsActive,
            ProfilePictureUrl = freshUser.ProfilePictureUrl,
            LastLoginAt = freshUser.LastLoginAt,
            CreatedAt = freshUser.CreatedAt,
            UpdatedAt = freshUser.UpdatedAt,
            OrganizationName = freshUser.Organization?.Name,
            OrganizationCode = freshUser.Organization?.Code
        };

        return ApiResponse<UserDto>.SuccessResponse(dto, "Profile updated successfully.");
    }
}
