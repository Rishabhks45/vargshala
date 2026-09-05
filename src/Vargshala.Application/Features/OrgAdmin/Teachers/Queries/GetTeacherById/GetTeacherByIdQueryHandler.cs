using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeacherById;

public class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, ApiResponse<TeacherDto>>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICurrentUser _currentUser;

    public GetTeacherByIdQueryHandler(
        ITeacherRepository teacherRepository,
        ICurrentUser currentUser)
    {
        _teacherRepository = teacherRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<TeacherDto>> Handle(
        GetTeacherByIdQuery query,
        CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdWithUserAsync(query.Id, cancellationToken);
        if (teacher == null)
        {
            return ApiResponse<TeacherDto>.FailureResponse("Teacher not found.");
        }

        var orgId = _currentUser.OrganizationId;
        if (orgId.HasValue && teacher.User?.OrganizationId != null && teacher.User.OrganizationId != orgId.Value)
        {
            return ApiResponse<TeacherDto>.FailureResponse("Unauthorized to view this teacher.");
        }

        return ApiResponse<TeacherDto>.SuccessResponse(teacher.ToDto());
    }
}
