using System.Security.Claims;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Common;

public static class RoleNavigationHelper
{
    public static string GetDefaultRouteForRole(UserRole role)
    {
        return role switch
        {
            UserRole.SuperAdmin => "/",
            UserRole.OrganizationAdmin => "/",
            UserRole.Teacher => "/attendance",
            UserRole.Student => "/study-material",
            _ => "/"
        };
    }

    public static string GetDefaultRouteForClaimsPrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return "/login";
        }

        var roleStr = principal.FindFirst(ClaimTypes.Role)?.Value;
        if (Enum.TryParse<UserRole>(roleStr, ignoreCase: true, out var role))
        {
            return GetDefaultRouteForRole(role);
        }

        return "/";
    }
}
