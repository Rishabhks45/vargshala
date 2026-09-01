using System.Security.Claims;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Abstractions.Authentication;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
