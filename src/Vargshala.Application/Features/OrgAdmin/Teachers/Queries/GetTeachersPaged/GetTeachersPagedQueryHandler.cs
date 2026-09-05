using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetTeachersPaged;

public class GetTeachersPagedQueryHandler : IRequestHandler<GetTeachersPagedQuery, ApiResponse<PagedResponse<TeacherDto>>>
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICurrentUser _currentUser;

    public GetTeachersPagedQueryHandler(
        ITeacherRepository teacherRepository,
        ICurrentUser currentUser)
    {
        _teacherRepository = teacherRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<TeacherDto>>> Handle(
        GetTeachersPagedQuery query,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<TeacherDto>>.FailureResponse("No active organization context found.");
        }

        var pagedRequest = query.Request ?? new PagedRequest();
        var (teachers, totalRecords) = await _teacherRepository.GetPagedByOrgAsync(
            orgId.Value,
            pagedRequest,
            query.Department,
            query.Designation,
            query.IsActive,
            cancellationToken);

        var dtos = teachers.Select(t => t.ToDto()).ToList();
        var response = PagedResponse<TeacherDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<TeacherDto>>.SuccessResponse(response);
    }
}
