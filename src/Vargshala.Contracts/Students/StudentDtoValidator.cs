using FluentValidation;

namespace Vargshala.Contracts.Students;

public class StudentDtoValidator : AbstractValidator<StudentDto>
{
    public StudentDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

        RuleFor(x => x.RollNumber)
            .NotEmpty().WithMessage("Roll Number is required.")
            .MaximumLength(30).WithMessage("Roll Number cannot exceed 30 characters.");

        RuleFor(x => x.BatchName)
            .NotEmpty().WithMessage("Batch assignment is required.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[+]?[\d\s-]{10,15}$").WithMessage("Enter a valid phone number (minimum 10 digits).");

        RuleFor(x => x.ParentName)
            .NotEmpty().WithMessage("Parent / Guardian name is required.")
            .MaximumLength(100).WithMessage("Parent name cannot exceed 100 characters.");

        RuleFor(x => x.TotalFee)
            .GreaterThanOrEqualTo(0).WithMessage("Total course fee cannot be negative.");

        RuleFor(x => x.PaidFee)
            .GreaterThanOrEqualTo(0).WithMessage("Paid amount cannot be negative.")
            .Must((student, paid) => paid <= student.TotalFee)
            .WithMessage("Paid amount cannot exceed total course fee.");
    }
}
