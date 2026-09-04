using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Vargshala.Web.Auth;

public class TokenValidator
{
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return null;

            var jwtToken = handler.ReadJwtToken(token);

            // Extract all claims from the verified JWT payload
            var identity = new ClaimsIdentity(
                jwtToken.Claims,
                "VargshalaAuth",
                ClaimTypes.Name,
                ClaimTypes.Role);

            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }
}
