using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Vargshala.Application.Abstractions.CurrentUser;

namespace Vargshala.Infrastructure.Authentication;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    public Guid? OrganizationId
    {
        get
        {
            var claim = User?.FindFirst("organization_id")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string Role => User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsSuperAdmin => Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
}
