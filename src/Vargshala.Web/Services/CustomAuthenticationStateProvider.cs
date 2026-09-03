using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Vargshala.Contracts.Authentication;

namespace Vargshala.Web.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    private ClaimsPrincipal _currentUser;
    private UserInfo? _cachedUser;
    private string? _cachedToken;
    private bool _isInitialized;

    public CustomAuthenticationStateProvider(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
        _currentUser = _anonymous;
    }

    public UserInfo? CurrentUser => _cachedUser;
    public string? CurrentToken => _cachedToken;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_isInitialized)
        {
            try
            {
                var tokenResult = await _localStorage.GetAsync<string>("authToken");
                var userResult = await _localStorage.GetAsync<UserInfo>("authUser");

                if (tokenResult.Success && !string.IsNullOrWhiteSpace(tokenResult.Value) && 
                    userResult.Success && userResult.Value is not null)
                {
                    _cachedToken = tokenResult.Value;
                    _cachedUser = userResult.Value;
                    _currentUser = CreateClaimsPrincipal(_cachedUser);
                }
                else
                {
                    _currentUser = _anonymous;
                }
            }
            catch
            {
                // Fallback to anonymous during prerender or storage error
                _currentUser = _anonymous;
            }

            _isInitialized = true;
        }

        return new AuthenticationState(_currentUser);
    }

    public async Task MarkUserAsAuthenticatedAsync(LoginResponse loginResponse)
    {
        _cachedToken = loginResponse.AccessToken;
        _cachedUser = loginResponse.User;
        _currentUser = CreateClaimsPrincipal(loginResponse.User);
        _isInitialized = true;

        try
        {
            await _localStorage.SetAsync("authToken", loginResponse.AccessToken);
            await _localStorage.SetAsync("authUser", loginResponse.User);
        }
        catch
        {
            // Ignored if JS interop not ready
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        _cachedToken = null;
        _cachedUser = null;
        _currentUser = _anonymous;
        _isInitialized = true;

        try
        {
            await _localStorage.DeleteAsync("authToken");
            await _localStorage.DeleteAsync("authUser");
        }
        catch
        {
            // Ignored if JS interop not ready
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(UserInfo user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("OrganizationId", user.OrganizationId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(user.OrganizationName))
        {
            claims.Add(new Claim("OrganizationName", user.OrganizationName));
        }

        var identity = new ClaimsIdentity(claims, "VargshalaAuth");
        return new ClaimsPrincipal(identity);
    }
}
