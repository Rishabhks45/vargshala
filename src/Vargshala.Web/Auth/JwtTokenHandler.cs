using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<string, (string AccessToken, string RefreshToken)> _tokenCache = new();
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

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

    public static void SetUserTokens(string userId, string accessToken, string refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(accessToken))
        {
            _tokenCache[userId] = (accessToken, refreshToken);
        }
    }

    public static void ClearUserTokens(string userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            _tokenCache.TryRemove(userId, out _);
        }
    }

    private string? GetCurrentAccessToken(HttpContext httpContext, string? userId)
    {
        if (!string.IsNullOrEmpty(userId) && _tokenCache.TryGetValue(userId, out var cached) && !string.IsNullOrEmpty(cached.AccessToken))
        {
            return cached.AccessToken;
        }

        return httpContext.User.FindFirst("access_token")?.Value;
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
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? httpContext.User.FindFirst("sub")?.Value;

            var accessToken = GetCurrentAccessToken(httpContext, userId);
            var refreshToken = (!string.IsNullOrEmpty(userId) && _tokenCache.TryGetValue(userId, out var cached))
                ? cached.RefreshToken
                : RefreshTokenNormalizer.Normalize(httpContext.User.FindFirst("refresh_token")?.Value);

            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(refreshToken))
            {
                await _refreshLock.WaitAsync(cancellationToken);
                try
                {
                    // Check if token was already refreshed by another concurrent thread
                    var latestToken = GetCurrentAccessToken(httpContext, userId);
                    if (!string.IsNullOrEmpty(latestToken) && latestToken != accessToken)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", latestToken);
                        response.Dispose();
                        return await base.SendAsync(request, cancellationToken);
                    }

                    var refreshed = await TryRefreshTokenAsync(accessToken ?? string.Empty, refreshToken, cancellationToken);
                    if (refreshed is not null)
                    {
                        await SignInWithTokensAsync(httpContext, userId, refreshed, cancellationToken);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
                        response.Dispose();
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
                finally
                {
                    _refreshLock.Release();
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
        string? userId,
        RefreshTokenResponse login,
        CancellationToken cancellationToken)
    {
        var principal = _tokenValidator.ValidateToken(login.AccessToken);
        if (principal is null)
            return;

        var resolvedUserId = userId
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(resolvedUserId))
        {
            SetUserTokens(resolvedUserId, login.AccessToken, RefreshTokenNormalizer.Normalize(login.RefreshToken));
        }

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
        claims.Add(new Claim("refresh_token", RefreshTokenNormalizer.Normalize(login.RefreshToken)));

        var newIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        // Update in-memory user principal on HttpContext for the active circuit
        httpContext.User = new ClaimsPrincipal(newIdentity);

        // In Blazor Server Interactive mode, circuits run over persistent WebSockets where Response.HasStarted is true.
        // Attempting HttpContext.SignInAsync when response headers have already started throws:
        // "Headers are read-only, response has already started."
        // We ONLY call SignInAsync if response headers have not started yet (e.g. during standard HTTP request).
        if (!httpContext.Response.HasStarted)
        {
            try
            {
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    httpContext.User,
                    new AuthenticationProperties { IsPersistent = true });
            }
            catch
            {
                // In case Response.HasStarted became true concurrently
            }
        }
    }
}
