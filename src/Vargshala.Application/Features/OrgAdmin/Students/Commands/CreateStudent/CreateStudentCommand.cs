using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Commands.CreateStudent;

public record CreateStudentCommand(CreateStudentRequest Request) : IRequest<ApiResponse<StudentDto>>;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new CreateStudentRequestValidator());
    }
}
