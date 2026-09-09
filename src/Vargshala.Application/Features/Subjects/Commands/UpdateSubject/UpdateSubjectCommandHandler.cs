using Mapster;
using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subjects;

namespace Vargshala.Application.Features.Subjects.Commands.UpdateSubject;

public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, ApiResponse<SubjectDto>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateSubjectCommandHandler(
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubjectDto>> Handle(
        UpdateSubjectCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<SubjectDto>.FailureResponse("Unauthorized. Organization context required.");
        }

        var orgId = _currentUser.OrganizationId.Value;
        var req = command.Request;

        var subject = await _subjectRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (subject is null || subject.OrganizationId != orgId)
        {
            return ApiResponse<SubjectDto>.FailureResponse("Subject not found.");
        }

        var codeExists = await _subjectRepository.ExistsByCodeAndOrgAsync(req.Code.Trim(), orgId, excludeId: req.Id, cancellationToken: cancellationToken);
        if (codeExists)
        {
            return ApiResponse<SubjectDto>.FailureResponse($"Another subject with code '{req.Code.Trim()}' already exists.");
        }

        subject.Name = req.Name.Trim();
        subject.Code = req.Code.Trim().ToUpperInvariant();
        subject.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        subject.IsActive = req.IsActive;
        subject.UpdatedAt = DateTime.UtcNow;
        subject.UpdatedBy = _currentUser.UserId;

        _subjectRepository.Update(subject);
        await _subjectRepository.SaveChangesAsync(cancellationToken);

        var dto = subject.Adapt<SubjectDto>();
        return ApiResponse<SubjectDto>.SuccessResponse(dto, "Subject updated successfully.");
    }
}
