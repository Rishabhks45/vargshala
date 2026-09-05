using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetStudentsPaged;

public class GetStudentsPagedQueryHandler : IRequestHandler<GetStudentsPagedQuery, ApiResponse<PagedResponse<StudentDto>>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUser _currentUser;

    public GetStudentsPagedQueryHandler(
        IStudentRepository studentRepository,
        ICurrentUser currentUser)
    {
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<StudentDto>>> Handle(
        GetStudentsPagedQuery query,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<PagedResponse<StudentDto>>.FailureResponse("No active organization context found.");
        }

        var pagedRequest = query.Request ?? new PagedRequest();
        var (students, totalRecords) = await _studentRepository.GetPagedByOrgAsync(
            orgId.Value,
            pagedRequest,
            query.ClassName,
            query.Section,
            query.IsActive,
            cancellationToken);

        var dtos = students.Select(s => s.ToDto()).ToList();
        var response = PagedResponse<StudentDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<StudentDto>>.SuccessResponse(response);
    }
}
