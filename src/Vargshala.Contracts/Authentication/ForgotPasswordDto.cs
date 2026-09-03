using FluentValidation;

namespace Vargshala.Contracts.Authentication;

#region Request
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");
    }
}
#endregion

#region Response
public class ForgotPasswordResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Password reset instructions sent if email exists.";
}
#endregion
