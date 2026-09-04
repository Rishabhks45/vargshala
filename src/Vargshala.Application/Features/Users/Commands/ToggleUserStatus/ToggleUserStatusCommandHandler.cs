using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Users.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, ApiResponse<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public ToggleUserStatusCommandHandler(
        IUserRepository userRepository,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        ToggleUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ApiResponse<bool>.FailureResponse("User not found.");
        }

        // Prevent self-deactivation
        if (_currentUser.UserId != Guid.Empty && user.Id == _currentUser.UserId)
        {
            return ApiResponse<bool>.FailureResponse("You cannot deactivate your own account.");
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = _currentUser.UserId;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var statusText = user.IsActive ? "Activated" : "Suspended";
        return ApiResponse<bool>.SuccessResponse(user.IsActive, $"User account has been {statusText}.");
    }
}
