using FluentValidation;

namespace Vargshala.Contracts.Teachers;

public class TeacherDtoValidator : AbstractValidator<TeacherDto>
{
    public TeacherDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Enter a valid email address.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Mobile)
            .MaximumLength(20).WithMessage("Mobile cannot exceed 20 characters.");

        RuleFor(x => x.EmployeeCode)
            .MaximumLength(50).WithMessage("Employee code cannot exceed 50 characters.");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.Designation)
            .MaximumLength(100).WithMessage("Designation cannot exceed 100 characters.");

        RuleFor(x => x.AadharNumber)
            .MaximumLength(20).WithMessage("Aadhar number cannot exceed 20 characters.");
    }
}
