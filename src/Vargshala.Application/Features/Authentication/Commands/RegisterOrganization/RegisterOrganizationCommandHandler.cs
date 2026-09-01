using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;
using Vargshala.Domain.Enums;

namespace Vargshala.Application.Features.Authentication.Commands.RegisterOrganization;

public class RegisterOrganizationCommandHandler
    : IRequestHandler<RegisterOrganizationCommand, ApiResponse<LoginResponse>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterOrganizationCommandHandler(
        IVargshalaDbContext db,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(
        RegisterOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        // Check if organization code already exists
        var codeExists = await _db.Organizations
            .AnyAsync(o => o.Code == request.OrganizationCode && !o.IsDeleted, cancellationToken);

        if (codeExists)
        {
            return ApiResponse<LoginResponse>.FailureResponse(
                "An organization with this code already exists.");
        }

        // Check if email already exists
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == request.AdminEmail && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<LoginResponse>.FailureResponse(
                "A user with this email already exists.");
        }

        // Create organization
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName,
            Code = request.OrganizationCode,
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
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = Role.OrganizationAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
            LastLoginAt = DateTime.UtcNow
        };

        _db.Organizations.Add(organization);
        _db.Users.Add(adminUser);
        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(adminUser);

        // Set navigation for response mapping
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
                Role = adminUser.Role.ToString(),
                OrganizationId = organization.Id,
                OrganizationName = organization.Name
            }
        };

        return ApiResponse<LoginResponse>.SuccessResponse(response, "Organization registered successfully.");
    }
}
