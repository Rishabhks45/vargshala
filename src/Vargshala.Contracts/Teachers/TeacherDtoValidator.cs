using FluentValidation;

namespace Vargshala.Contracts.Teachers;

public class TeacherDtoValidator : AbstractValidator<TeacherDto>
{
    public TeacherDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Enter a valid email address.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[+]?[\d\s-]{10,15}$").WithMessage("Enter a valid phone number (minimum 10 digits).");

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Designation is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Primary subject is required.");

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required.");
    }
}
