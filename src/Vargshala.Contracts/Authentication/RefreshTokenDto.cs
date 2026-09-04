using FluentValidation;

namespace Vargshala.Contracts.Authentication;

#region Request
public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
#endregion

#region Response
public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
}
#endregion

/// <summary>
/// Query-string parsers treat '+' as space (application/x-www-form-urlencoded).
/// Restore '+' before comparing or storing a refresh token from a URL or cookie.
/// </summary>
public static class RefreshTokenNormalizer
{
    public static string Normalize(string? token) =>
        string.IsNullOrEmpty(token) ? string.Empty : token.Trim().Replace(' ', '+');
}
