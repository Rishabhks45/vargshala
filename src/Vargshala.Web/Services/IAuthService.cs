using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync();
    UserInfo? GetCurrentUser();
}

