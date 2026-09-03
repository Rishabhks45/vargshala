using FluentValidation;
using MediatR;
using Vargshala.Contracts.Authentication;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Authentication.Commands.RegisterUser;

public record RegisterUserCommand(
    string OrganizationCode,
    string FirstName,
    string LastName,
    string Email,
    string? Mobile,
    string Password,
    UserRole Role = UserRole.Student
) : IRequest<ApiResponse<LoginResponse>>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.OrganizationCode)
            .NotEmpty().WithMessage("Organization code is required.")
            .MaximumLength(50).WithMessage("Organization code cannot exceed 50 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
            .EmailAddress().WithMessage("Enter a valid email address.");

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile number cannot exceed 20 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Valid user role is required.");
    }
}
