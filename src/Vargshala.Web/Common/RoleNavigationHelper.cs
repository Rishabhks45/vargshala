using System.Security.Claims;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Common;

public static class RoleNavigationHelper
{
    public static string GetDefaultRouteForRole(UserRole role)
    {
        return role switch
        {
            UserRole.SuperAdmin => "/controlpanel/platform",
            UserRole.BackOffice => "/controlpanel/platform",
            UserRole.OrganizationAdmin => "/",
            UserRole.BranchAdmin => "/",
            UserRole.Teacher => "/attendance",
            UserRole.Student => "/student/home",
            _ => "/"
        };
    }

    public static string GetDefaultRouteForClaimsPrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return "/login";
        }

        if (principal.IsInRole("SuperAdmin") || principal.IsInRole("1001") ||
            principal.IsInRole("BackOffice") || principal.IsInRole("1002"))
        {
            return "/controlpanel/platform";
        }

        if (principal.IsInRole("Student") || principal.IsInRole("3"))
        {
            return "/student/home";
        }

        if (principal.IsInRole("Teacher") || principal.IsInRole("2"))
        {
            return "/attendance";
        }

        if (principal.IsInRole("OrganizationAdmin") || principal.IsInRole("1") ||
            principal.IsInRole("BranchAdmin") || principal.IsInRole("4"))
        {
            return "/";
        }

        var roleStr = principal.FindFirst(ClaimTypes.Role)?.Value 
                   ?? principal.FindFirst("role")?.Value 
                   ?? principal.FindFirst("Role")?.Value;

        if (Enum.TryParse<UserRole>(roleStr, ignoreCase: true, out var role))
        {
            return GetDefaultRouteForRole(role);
        }

        if (int.TryParse(roleStr, out var roleInt) && Enum.IsDefined(typeof(UserRole), roleInt))
        {
            return GetDefaultRouteForRole((UserRole)roleInt);
        }

        return "/";
    }

    public static string GetRoleDisplayName(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return "User";
        }

        var roleStr = principal.FindFirst(ClaimTypes.Role)?.Value 
                   ?? principal.FindFirst("role")?.Value 
                   ?? principal.FindFirst("Role")?.Value;

        if (string.IsNullOrWhiteSpace(roleStr))
        {
            return "User";
        }

        if (Enum.TryParse<UserRole>(roleStr, ignoreCase: true, out var role))
        {
            return role.GetDisplayName();
        }

        if (int.TryParse(roleStr, out var roleInt) && Enum.IsDefined(typeof(UserRole), roleInt))
        {
            return ((UserRole)roleInt).GetDisplayName();
        }

        return roleStr;
    }
}
