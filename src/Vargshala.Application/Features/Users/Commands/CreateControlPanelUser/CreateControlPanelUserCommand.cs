using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.Application.Features.Users.Commands.CreateControlPanelUser;

public record CreateControlPanelUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Mobile,
    string Password,
    UserRole Role,
    Guid? OrganizationId
) : IRequest<ApiResponse<UserDto>>;

public class CreateControlPanelUserCommandValidator : AbstractValidator<CreateControlPanelUserCommand>
{
    public CreateControlPanelUserCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.Role)
            .Must(r => r == UserRole.SuperAdmin || r == UserRole.BackOffice || r == UserRole.OrganizationAdmin)
            .WithMessage("Role must be SuperAdmin, BackOffice, or OrganizationAdmin.");

        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Please select an organization for OrganizationAdmin.")
            .When(x => x.Role == UserRole.OrganizationAdmin);

        RuleFor(x => x.OrganizationId)
            .Empty().WithMessage("Platform users (SuperAdmin / BackOffice) cannot be assigned to an organization.")
            .When(x => x.Role == UserRole.SuperAdmin || x.Role == UserRole.BackOffice);
    }
}
