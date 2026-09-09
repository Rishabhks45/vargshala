using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Queries.GetAllActiveSubjects;

public class GetAllActiveSubjectsQueryHandler : IRequestHandler<GetAllActiveSubjectsQuery, ApiResponse<List<SubjectDto>>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public GetAllActiveSubjectsQueryHandler(
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<SubjectDto>>> Handle(
        GetAllActiveSubjectsQuery query,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<List<SubjectDto>>.FailureResponse("Unauthorized.");
        }

        var subjects = await _subjectRepository.GetAllActiveByOrgAsync(_currentUser.OrganizationId.Value, cancellationToken);
        var dtos = subjects.Select(s => s.Adapt<SubjectDto>()).ToList();

        return ApiResponse<List<SubjectDto>>.SuccessResponse(dtos);
    }
}
