using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Commands.UpdateTeacher;

public record UpdateTeacherCommand(UpdateTeacherRequest Request) : IRequest<ApiResponse<TeacherDto>>;

public class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new UpdateTeacherRequestValidator());
    }
}
