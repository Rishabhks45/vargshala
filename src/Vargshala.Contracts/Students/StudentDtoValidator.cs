using FluentValidation;

namespace Vargshala.Contracts.Students;

public class StudentDtoValidator : AbstractValidator<StudentDto>
{
    public StudentDtoValidator()
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

        RuleFor(x => x.StudentCode)
            .MaximumLength(50).WithMessage("Student code cannot exceed 50 characters.");

        RuleFor(x => x.RollNumber)
            .MaximumLength(50).WithMessage("Roll number cannot exceed 50 characters.");

        RuleFor(x => x.ClassName)
            .MaximumLength(100).WithMessage("Class name cannot exceed 100 characters.");

        RuleFor(x => x.Section)
            .MaximumLength(50).WithMessage("Section cannot exceed 50 characters.");

        RuleFor(x => x.FatherName)
            .MaximumLength(150).WithMessage("Father name cannot exceed 150 characters.");

        RuleFor(x => x.MotherName)
            .MaximumLength(150).WithMessage("Mother name cannot exceed 150 characters.");

        RuleFor(x => x.FatherMobile)
            .MaximumLength(20).WithMessage("Father mobile cannot exceed 20 characters.");

        RuleFor(x => x.EmergencyContactMobile)
            .MaximumLength(20).WithMessage("Emergency contact mobile cannot exceed 20 characters.");

        RuleFor(x => x.AadharNumber)
            .MaximumLength(20).WithMessage("Aadhar number cannot exceed 20 characters.");
    }
}
