using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Commands.UpdateStudent;

public record UpdateStudentCommand(UpdateStudentRequest Request) : IRequest<ApiResponse<StudentDto>>;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new UpdateStudentRequestValidator());
    }
}
