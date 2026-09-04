using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;
using Vargshala.Web.Auth;
using Vargshala.Web.Common;

namespace Vargshala.Web.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly TokenValidator _tokenValidator;

    public AccountController(TokenValidator tokenValidator)
    {
        _tokenValidator = tokenValidator;
    }

    /// <summary>
    /// Browser redirect sign-in (sets auth cookie on the user's actual HTTP request).
    /// </summary>
    [HttpGet("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(
        [FromQuery] string token,
        [FromQuery] string? refreshToken,
        [FromQuery] bool rememberMe = false,
        [FromQuery] string? orgName = null,
        [FromQuery] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Redirect("/login");

        var principal = _tokenValidator.ValidateToken(token);
        if (principal is null)
            return Redirect("/login");

        var claims = principal.Claims.ToList();

        // Extract role claim
        var roleClaim = claims.FirstOrDefault(c => c.Type is ClaimTypes.Role or "role" or "Role")?.Value;
        UserRole userRole = UserRole.OrganizationAdmin;
        if (!string.IsNullOrWhiteSpace(roleClaim) && Enum.TryParse<UserRole>(roleClaim, ignoreCase: true, out var parsedRole))
        {
            userRole = parsedRole;
        }
        else if (int.TryParse(roleClaim, out var roleInt) && Enum.IsDefined(typeof(UserRole), roleInt))
        {
            userRole = (UserRole)roleInt;
        }

        // Ensure both string and integer forms of role are present for compatibility
        if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == userRole.ToString()))
            claims.Add(new Claim(ClaimTypes.Role, userRole.ToString()));

        if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == ((int)userRole).ToString()))
            claims.Add(new Claim(ClaimTypes.Role, ((int)userRole).ToString()));

        if (!claims.Any(c => c.Type == "role" && c.Value == userRole.ToString()))
            claims.Add(new Claim("role", userRole.ToString()));

        // Add organization name if provided
        if (!string.IsNullOrWhiteSpace(orgName) && !claims.Any(c => c.Type == "OrganizationName"))
        {
            claims.Add(new Claim("OrganizationName", orgName));
        }

        claims.Add(new Claim("access_token", token));

        var normalizedRefreshToken = RefreshTokenNormalizer.Normalize(refreshToken);
        if (!string.IsNullOrEmpty(normalizedRefreshToken))
            claims.Add(new Claim("refresh_token", normalizedRefreshToken));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        var expiry = rememberMe
            ? DateTimeOffset.UtcNow.AddDays(7)
            : DateTimeOffset.UtcNow.AddHours(24);

        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                     ?? claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            JwtTokenHandler.SetUserTokens(userId, token, normalizedRefreshToken);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = expiry
            });

        // Determine target route if none specified
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = RoleNavigationHelper.GetDefaultRouteForRole(userRole);
        }

        if (!returnUrl.StartsWith("/"))
        {
            returnUrl = "/" + returnUrl;
        }

        return LocalRedirect(returnUrl);
    }

    [HttpGet("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            JwtTokenHandler.ClearUserTokens(userId);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}
