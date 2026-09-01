using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IVargshalaDbContext db,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            return ApiResponse<LoginResponse>.FailureResponse("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponse>.FailureResponse("Your account has been deactivated. Please contact your administrator.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponse>.FailureResponse("Invalid email or password.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token on user
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

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
                Role = user.Role.ToString(),
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization?.Name
            }
        };

        return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful.");
    }
}
