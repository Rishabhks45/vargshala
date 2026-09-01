using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponse>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IVargshalaDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return ApiResponse<RefreshTokenResponse>.FailureResponse("Invalid access token.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ApiResponse<RefreshTokenResponse>.FailureResponse("Invalid token claims.");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user is null || user.RefreshToken != request.RefreshToken
            || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<RefreshTokenResponse>.FailureResponse("Invalid or expired refresh token.");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _db.SaveChangesAsync(cancellationToken);

        return ApiResponse<RefreshTokenResponse>.SuccessResponse(new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30)
        });
    }
}
