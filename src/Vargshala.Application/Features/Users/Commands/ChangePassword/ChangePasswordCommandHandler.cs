using MediatR;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public async Task<ApiResponse<bool>> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == Guid.Empty)
        {
            return ApiResponse<bool>.FailureResponse("User not authenticated.");
        }

        var req = command.Request;
        var user = await _userRepository.GetByIdForUpdateAsync(_currentUser.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse<bool>.FailureResponse("User profile not found.");
        }

        // Validate current password
        try
        {
            var decryptedPassword = _encryptionService.Decrypt(user.PasswordHash, _encryptionSettings.MasterKey);
            if (req.CurrentPassword != decryptedPassword)
            {
                return ApiResponse<bool>.FailureResponse("The current password you entered is incorrect.");
            }
        }
        catch
        {
            return ApiResponse<bool>.FailureResponse("Unable to verify current password.");
        }

        // Encrypt and update new password
        user.PasswordHash = _encryptionService.Encrypt(req.NewPassword, _encryptionSettings.MasterKey);
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully.");
    }
}
