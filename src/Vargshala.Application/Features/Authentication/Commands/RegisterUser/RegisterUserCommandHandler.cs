using MediatR;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Authentication.Commands.RegisterUser;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, ApiResponse<LoginResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public RegisterUserCommandHandler(
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

    public async Task<ApiResponse<LoginResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate Organization existence
        var organization = await _authRepository.GetOrganizationByCodeAsync(request.OrganizationCode, cancellationToken);
        if (organization is null)
        {
            return ApiResponse<LoginResponse>.FailureResponse("Organization not found. Please check your organization code.");
        }

        if (!organization.IsActive)
        {
            return ApiResponse<LoginResponse>.FailureResponse("This organization account is currently inactive.");
        }

        // 2. Validate Role
        if (request.Role is UserRole.SuperAdmin or UserRole.OrganizationAdmin)
        {
            return ApiResponse<LoginResponse>.FailureResponse("Self-registration as administrator is not permitted.");
        }

        // 3. Check for duplicate email in this organization
        var emailExists = await _authRepository.UserEmailExistsInOrgAsync(request.Email, organization.Id, cancellationToken);
        if (emailExists)
        {
            return ApiResponse<LoginResponse>.FailureResponse("A user with this email already exists in this organization.");
        }

        // 4. Create User
        var refreshToken = _tokenService.GenerateRefreshToken();
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Mobile = request.Mobile,
            PasswordHash = _encryptionService.Encrypt(request.Password, _encryptionSettings.MasterKey),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
            LastLoginAt = DateTime.UtcNow
        };

        await _authRepository.AddUserAsync(user, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);
        user.Organization = organization;

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
                OrganizationId = organization.Id,
                OrganizationName = organization.Name
            }
        };

        return ApiResponse<LoginResponse>.SuccessResponse(response, "User registered successfully.");
    }
}
