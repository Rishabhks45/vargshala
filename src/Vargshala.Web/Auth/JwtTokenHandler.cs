using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Auth;

public class JwtTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _clientFactory;
    private readonly TokenValidator _tokenValidator;

    public JwtTokenHandler(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory clientFactory,
        TokenValidator tokenValidator)
    {
        _httpContextAccessor = httpContextAccessor;
        _clientFactory = clientFactory;
        _tokenValidator = tokenValidator;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath.Contains("refresh", StringComparison.OrdinalIgnoreCase) == true ||
            request.RequestUri?.AbsolutePath.Contains("login", StringComparison.OrdinalIgnoreCase) == true ||
            request.RequestUri?.AbsolutePath.Contains("register", StringComparison.OrdinalIgnoreCase) == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var accessToken = httpContext.User.FindFirst("access_token")?.Value;
            var refreshToken = httpContext.User.FindFirst("refresh_token")?.Value;

            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(refreshToken))
            {
                var refreshed = await TryRefreshTokenAsync(accessToken!, refreshToken, cancellationToken);
                if (refreshed is not null)
                {
                    await SignInWithTokensAsync(httpContext, refreshed, cancellationToken);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
                    response.Dispose();
                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<RefreshTokenResponse?> TryRefreshTokenAsync(
        string accessToken,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var client = _clientFactory.CreateClient("VargshalaApi.Anonymous");
        var payload = new RefreshTokenRequest
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        var result = await client.PostAsJsonAsync("api/v1/auth/refresh", payload, cancellationToken);
        if (!result.IsSuccessStatusCode)
            return null;

        var response = await result.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>(cancellationToken: cancellationToken);
        return response is { Success: true, Data: not null } ? response.Data : null;
    }

    private async Task SignInWithTokensAsync(
        HttpContext httpContext,
        RefreshTokenResponse login,
        CancellationToken cancellationToken)
    {
        var principal = _tokenValidator.ValidateToken(login.AccessToken);
        if (principal is null)
            return;

        var claims = principal.Claims
            .Where(c => c.Type is not "access_token" and not "refresh_token")
            .ToList();

        foreach (var roleClaim in claims.Where(c => c.Type == "roles" || c.Type == "role").ToList())
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == roleClaim.Value))
                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
        }

        if (!claims.Any(c => c.Type == ClaimTypes.Name))
        {
            var nameClaim = claims.FirstOrDefault(c => c.Type is "name" or ClaimTypes.Name);
            if (nameClaim is not null)
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
        }

        var currentOrgClaim = httpContext.User.FindFirst("OrganizationName")?.Value;
        if (!string.IsNullOrEmpty(currentOrgClaim) && !claims.Any(c => c.Type == "OrganizationName"))
        {
            claims.Add(new Claim("OrganizationName", currentOrgClaim));
        }

        claims.Add(new Claim("access_token", login.AccessToken));
        claims.Add(new Claim("refresh_token", login.RefreshToken));

        var newIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(newIdentity),
            new AuthenticationProperties { IsPersistent = true });
    }
}
