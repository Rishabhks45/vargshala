using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Commands.CreateTeacher;

public record CreateTeacherCommand(CreateTeacherRequest Request) : IRequest<ApiResponse<TeacherDto>>;

public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new CreateTeacherRequestValidator());
    }
}
