using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Auth;

/// <summary>
/// HTTP message handler that attaches the JWT Bearer token to outgoing API requests
/// and automatically refreshes expired tokens on 401 Unauthorized.
/// </summary>
public class JwtTokenHandler : DelegatingHandler
{
    private static readonly ConcurrentDictionary<string, (string AccessToken, string RefreshToken)> _tokenCache = new();
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _clientFactory;

    public JwtTokenHandler(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory clientFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _clientFactory = clientFactory;
    }

    public static void SetUserTokens(string userId, string accessToken, string refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(accessToken))
            _tokenCache[userId] = (accessToken, refreshToken);
    }

    public static void ClearUserTokens(string userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
            _tokenCache.TryRemove(userId, out _);
    }

    public static string? GetUserAccessToken(string userId) =>
        !string.IsNullOrWhiteSpace(userId) && _tokenCache.TryGetValue(userId, out var cached) ? cached.AccessToken : null;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (path.Contains("refresh", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("login", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("register", StringComparison.OrdinalIgnoreCase))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        string? userId = null;
        string? accessToken = null;
        string? refreshToken = null;

        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                accessToken = user.FindFirst("access_token")?.Value;
                refreshToken = RefreshTokenNormalizer.Normalize(user.FindFirst("refresh_token")?.Value);
            }
        }
        catch (ObjectDisposedException) { }

        // Prefer latest cached tokens if available
        if (!string.IsNullOrEmpty(userId) && _tokenCache.TryGetValue(userId, out var cached))
        {
            accessToken = cached.AccessToken;
            refreshToken = cached.RefreshToken;
        }

        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await base.SendAsync(request, cancellationToken);

        // Auto-refresh token on 401 Unauthorized and retry request
        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(refreshToken))
        {
            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                // Check if already refreshed by another concurrent request
                if (!string.IsNullOrEmpty(userId) && _tokenCache.TryGetValue(userId, out var latest) && latest.AccessToken != accessToken)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", latest.AccessToken);
                    response.Dispose();
                    return await base.SendAsync(request, cancellationToken);
                }

                var client = _clientFactory.CreateClient("VargshalaApi.Anonymous");
                var refreshResp = await client.PostAsJsonAsync("api/v1/auth/refresh", new RefreshTokenRequest
                {
                    AccessToken = accessToken ?? string.Empty,
                    RefreshToken = refreshToken
                }, cancellationToken);

                if (refreshResp.IsSuccessStatusCode)
                {
                    var result = await refreshResp.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>(cancellationToken);
                    if (result is { Success: true, Data: not null })
                    {
                        var newAccess = result.Data.AccessToken;
                        var newRefresh = RefreshTokenNormalizer.Normalize(result.Data.RefreshToken);

                        if (!string.IsNullOrEmpty(userId))
                            SetUserTokens(userId, newAccess, newRefresh);

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccess);
                        response.Dispose();
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        return response;
    }
}
