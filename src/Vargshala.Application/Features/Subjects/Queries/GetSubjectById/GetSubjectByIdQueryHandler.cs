using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Queries.GetSubjectById;

public class GetSubjectByIdQueryHandler : IRequestHandler<GetSubjectByIdQuery, ApiResponse<SubjectDto>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public GetSubjectByIdQueryHandler(
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubjectDto>> Handle(
        GetSubjectByIdQuery query,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<SubjectDto>.FailureResponse("Unauthorized.");
        }

        var subject = await _subjectRepository.GetByIdAsync(query.Id, cancellationToken);
        if (subject is null || subject.OrganizationId != _currentUser.OrganizationId.Value)
        {
            return ApiResponse<SubjectDto>.FailureResponse("Subject not found.");
        }

        return ApiResponse<SubjectDto>.SuccessResponse(subject.Adapt<SubjectDto>());
    }
}
