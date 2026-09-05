using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Students.Commands.DeleteStudent;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, ApiResponse<bool>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteStudentCommandHandler(
        IStudentRepository studentRepository,
        ICurrentUser currentUser)
    {
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteStudentCommand command,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (student == null)
        {
            return ApiResponse<bool>.FailureResponse("Student not found.");
        }

        var orgId = _currentUser.OrganizationId;
        if (orgId.HasValue && student.User?.OrganizationId != null && student.User.OrganizationId != orgId.Value)
        {
            return ApiResponse<bool>.FailureResponse("Unauthorized to delete this student.");
        }

        student.DeletedBy = _currentUser.UserId;
        _studentRepository.Delete(student);
        await _studentRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Student deleted successfully.");
    }
}
