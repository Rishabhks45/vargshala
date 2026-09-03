using FluentValidation;

namespace Vargshala.Contracts.Authentication;

#region Request
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .DependentRules(() =>
            {
                RuleFor(x => x.ConfirmPassword)
                    .NotEmpty().WithMessage("Please confirm your password.")
                    .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
            });
    }
}
#endregion

#region Response
public class ResetPasswordResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Password has been reset successfully. You can now login.";
}
#endregion
