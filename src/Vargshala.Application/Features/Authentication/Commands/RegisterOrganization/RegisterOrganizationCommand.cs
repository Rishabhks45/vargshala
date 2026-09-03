using FluentValidation;
using MediatR;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.RegisterOrganization;

public record RegisterOrganizationCommand(
    string OrganizationName,
    string OrganizationCode,
    string? LogoUrl,
    string? Email,
    string? Mobile,
    string? Address,
    string? City,
    string? State,
    string? Pincode,
    string? AcademicSession,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    string? AdminMobile,
    string Password
) : IRequest<ApiResponse<LoginResponse>>;

public class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
{
    public RegisterOrganizationCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200).WithMessage("Organization name must not exceed 200 characters.");

        RuleFor(x => x.OrganizationCode)
            .NotEmpty().WithMessage("Organization code is required.")
            .MaximumLength(50).WithMessage("Organization code must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Organization code must contain only letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage("Admin first name is required.")
            .MaximumLength(100);

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage("Admin last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
