using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentById;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, ApiResponse<StudentDto>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUser _currentUser;

    public GetStudentByIdQueryHandler(
        IStudentRepository studentRepository,
        ICurrentUser currentUser)
    {
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<StudentDto>> Handle(
        GetStudentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdWithUserAsync(query.Id, cancellationToken);
        if (student == null)
        {
            return ApiResponse<StudentDto>.FailureResponse("Student not found.");
        }

        var orgId = _currentUser.OrganizationId;
        if (orgId.HasValue && student.User?.OrganizationId != null && student.User.OrganizationId != orgId.Value)
        {
            return ApiResponse<StudentDto>.FailureResponse("Unauthorized to view this student.");
        }

        return ApiResponse<StudentDto>.SuccessResponse(student.ToDto());
    }
}
