using MediatR;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public LoginCommandHandler(
        IAuthRepository authRepository,
        ITokenService tokenService,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _authRepository.GetUserByEmailWithOrgAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ApiResponse<LoginResponse>.FailureResponse("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponse>.FailureResponse("Your account has been deactivated. Please contact your administrator.");
        }

        try
        {
            var decryptedPassword = _encryptionService.Decrypt(user.PasswordHash, _encryptionSettings.MasterKey);
            if (request.Password != decryptedPassword)
            {
                return ApiResponse<LoginResponse>.FailureResponse("Invalid email or password.");
            }
        }
        catch
        {
            return ApiResponse<LoginResponse>.FailureResponse("Invalid email or password.");
        }

        Guid? branchIdForToken = null;
        string? branchName = null;

        if (user.Role == UserRole.BranchAdmin)
        {
            var activeBranches = user.UserBranchAccesses
                .Where(a => a.IsActive && a.Branch != null && !a.Branch.IsDeleted)
                .ToList();

            if (activeBranches.Count == 0)
            {
                return ApiResponse<LoginResponse>.FailureResponse(
                    "No active branch is assigned to this Branch Admin account. Please contact your Institute Administrator.");
            }

            if (activeBranches.Count > 1)
            {
                return ApiResponse<LoginResponse>.FailureResponse(
                    "Configuration Conflict: Multiple active branch assignments detected for this Branch Admin account. Exactly one active branch is permitted. Please contact your Institute Administrator.");
            }

            branchIdForToken = activeBranches[0].BranchId;
            branchName = activeBranches[0].Branch?.Name;
        }
        else
        {
            var activeAccess = user.UserBranchAccesses
                .FirstOrDefault(a => a.Branch != null && a.Branch.IsMainBranch && !a.Branch.IsDeleted)
                ?? user.UserBranchAccesses.FirstOrDefault(a => a.Branch != null && !a.Branch.IsDeleted);

            branchIdForToken = activeAccess?.BranchId;
            branchName = activeAccess?.Branch?.Name;
        }

        var accessToken = _tokenService.GenerateAccessToken(user, user.Role == UserRole.BranchAdmin ? branchIdForToken : null);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token on user
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30),
            User = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization?.Name,
                CurrentBranchId = branchIdForToken,
                CurrentBranchName = branchName
            }
        };

        return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful.");
    }
}
