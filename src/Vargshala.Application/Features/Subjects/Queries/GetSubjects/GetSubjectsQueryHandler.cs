using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Queries.GetSubjects;

public class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, ApiResponse<PagedResponse<SubjectDto>>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public GetSubjectsQueryHandler(
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<PagedResponse<SubjectDto>>> Handle(
        GetSubjectsQuery query,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<PagedResponse<SubjectDto>>.FailureResponse("No organization associated with this user.");
        }

        var pagedRequest = query.Request ?? new PagedRequest();
        var (items, totalRecords) = await _subjectRepository.GetPagedByOrgAsync(
            _currentUser.OrganizationId.Value,
            pagedRequest,
            query.IsActive,
            cancellationToken);

        var dtos = items.Select(s => s.Adapt<SubjectDto>()).ToList();
        var response = PagedResponse<SubjectDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);

        return ApiResponse<PagedResponse<SubjectDto>>.SuccessResponse(response);
    }
}
