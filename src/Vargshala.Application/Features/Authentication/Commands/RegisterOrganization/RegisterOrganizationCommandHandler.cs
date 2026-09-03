using MediatR;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Authentication.Commands.RegisterOrganization;

public class RegisterOrganizationCommandHandler
    : IRequestHandler<RegisterOrganizationCommand, ApiResponse<LoginResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public RegisterOrganizationCommandHandler(
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
        RegisterOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        // Check if organization code already exists
        var codeExists = await _authRepository.OrganizationCodeExistsAsync(request.OrganizationCode, cancellationToken);
        if (codeExists)
        {
            return ApiResponse<LoginResponse>.FailureResponse("An organization with this code already exists.");
        }

        // Check if email already exists
        var emailExists = await _authRepository.UserEmailExistsAsync(request.AdminEmail, cancellationToken);
        if (emailExists)
        {
            return ApiResponse<LoginResponse>.FailureResponse("A user with this email already exists.");
        }

        // Create organization
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName,
            Code = request.OrganizationCode,
            LogoUrl = request.LogoUrl,
            Email = request.Email,
            Mobile = request.Mobile,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Pincode = request.Pincode,
            AcademicSession = request.AcademicSession,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create admin user
        var refreshToken = _tokenService.GenerateRefreshToken();
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            FirstName = request.AdminFirstName,
            LastName = request.AdminLastName,
            Email = request.AdminEmail,
            Mobile = request.AdminMobile,
            PasswordHash = _encryptionService.Encrypt(request.Password, _encryptionSettings.MasterKey),
            Role = UserRole.OrganizationAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
            LastLoginAt = DateTime.UtcNow
        };

        await _authRepository.AddOrganizationAsync(organization, cancellationToken);
        await _authRepository.AddUserAsync(adminUser, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(adminUser);
        adminUser.Organization = organization;

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30),
            User = new UserInfo
            {
                Id = adminUser.Id,
                FirstName = adminUser.FirstName,
                LastName = adminUser.LastName,
                Email = adminUser.Email,
                Role = adminUser.Role,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name
            }
        };

        return ApiResponse<LoginResponse>.SuccessResponse(response, "Organization registered successfully.");
    }
}
