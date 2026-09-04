using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigation;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        NavigationManager navigation,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VargshalaApi");
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

    public Task LogoutAsync()
    {
        _navigation.NavigateTo("/account/logout", forceLoad: true);
        return Task.CompletedTask;
    }

    public UserInfo? GetCurrentUser()
    {
        return null;
    }
}
