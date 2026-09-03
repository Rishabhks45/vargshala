using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly NavigationManager _navigation;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        CustomAuthenticationStateProvider authStateProvider,
        NavigationManager navigation,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
        _authStateProvider = authStateProvider;
        _navigation = navigation;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> RegisterOrganizationAsync(
        RegisterOrganizationRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register-organization", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(cancellationToken: cancellationToken);
            
            if (result is not null)
            {
                if (result.Success && result.Data is not null)
                {
                    await _authStateProvider.MarkUserAsAuthenticatedAsync(result.Data);
                }

                return result;
            }

            return ApiResponse<LoginResponse>.FailureResponse($"Server returned status code {(int)response.StatusCode}.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP connection error during organization registration");
            return ApiResponse<LoginResponse>.FailureResponse("Unable to connect to the backend server. Please make sure the API is running.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during organization registration");
            return ApiResponse<LoginResponse>.FailureResponse("Registration failed: " + ex.Message);
        }
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(
        LoginRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(cancellationToken: cancellationToken);

            if (result is not null)
            {
                if (result.Success && result.Data is not null)
                {
                    await _authStateProvider.MarkUserAsAuthenticatedAsync(result.Data);
                }

                return result;
            }

            return ApiResponse<LoginResponse>.FailureResponse($"Server returned status code {(int)response.StatusCode}.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP connection error during login");
            return ApiResponse<LoginResponse>.FailureResponse("Unable to connect to the backend server. Please make sure the API is running.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return ApiResponse<LoginResponse>.FailureResponse("Login failed: " + ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.MarkUserAsLoggedOutAsync();
        _navigation.NavigateTo("/login", replace: true);
    }

    public UserInfo? GetCurrentUser()
    {
        return _authStateProvider.CurrentUser;
    }
}
