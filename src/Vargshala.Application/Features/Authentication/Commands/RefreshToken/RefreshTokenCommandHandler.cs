using System.Security.Claims;
using MediatR;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Features.Authentication.Infrastructure;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponse>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IAuthRepository authRepository, ITokenService tokenService)
    {
        _authRepository = authRepository;
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

        var user = await _authRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return ApiResponse<RefreshTokenResponse>.FailureResponse("User not found.");
        }

        var incomingToken = RefreshTokenNormalizer.Normalize(request.RefreshToken);
        var storedToken = RefreshTokenNormalizer.Normalize(user.RefreshToken);

        if (string.IsNullOrEmpty(storedToken)
            || incomingToken != storedToken
            || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<RefreshTokenResponse>.FailureResponse("Invalid or expired refresh token.");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);

        // In Blazor Server Interactive mode, auth cookies cannot be updated over WebSockets mid-circuit.
        // Maintain the active refresh token for its duration (7 days), renewing the access token.
        if (user.RefreshTokenExpiryTime < DateTime.UtcNow.AddDays(1))
        {
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _authRepository.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<RefreshTokenResponse>.SuccessResponse(new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = user.RefreshToken ?? string.Empty,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30)
        });
    }
}
