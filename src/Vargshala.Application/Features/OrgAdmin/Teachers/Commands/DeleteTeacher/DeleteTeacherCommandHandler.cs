using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Commands.DeleteTeacher;

public class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand, ApiResponse<bool>>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteTeacherCommandHandler(
        ITeacherRepository teacherRepository,
        ICurrentUser currentUser)
    {
        _teacherRepository = teacherRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteTeacherCommand command,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (teacher == null)
        {
            return ApiResponse<bool>.FailureResponse("Teacher not found.");
        }

        var orgId = _currentUser.OrganizationId;
        if (orgId.HasValue && teacher.User?.OrganizationId != null && teacher.User.OrganizationId != orgId.Value)
        {
            return ApiResponse<bool>.FailureResponse("Unauthorized to delete this teacher.");
        }

        teacher.DeletedBy = _currentUser.UserId;
        _teacherRepository.Delete(teacher);
        await _teacherRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Teacher deleted successfully.");
    }
}
