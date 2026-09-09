using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Commands.CreateSubject;

public record CreateSubjectCommand(CreateSubjectRequest Request) : IRequest<ApiResponse<SubjectDto>>;

public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(150).WithMessage("Subject name cannot exceed 150 characters.");

        RuleFor(x => x.Request.Code)
            .NotEmpty().WithMessage("Subject code is required.")
            .MaximumLength(50).WithMessage("Subject code cannot exceed 50 characters.");

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));
    }
}
