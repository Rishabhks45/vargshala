using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Subjects.Commands.ToggleSubjectStatus;

public class ToggleSubjectStatusCommandHandler : IRequestHandler<ToggleSubjectStatusCommand, ApiResponse<bool>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public ToggleSubjectStatusCommandHandler(
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        ToggleSubjectStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUser.OrganizationId is null)
        {
            return ApiResponse<bool>.FailureResponse("Unauthorized.");
        }

        var subject = await _subjectRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (subject is null || subject.OrganizationId != _currentUser.OrganizationId.Value)
        {
            return ApiResponse<bool>.FailureResponse("Subject not found.");
        }

        subject.IsActive = !subject.IsActive;
        subject.UpdatedAt = DateTime.UtcNow;
        subject.UpdatedBy = _currentUser.UserId;

        _subjectRepository.Update(subject);
        await _subjectRepository.SaveChangesAsync(cancellationToken);

        var status = subject.IsActive ? "activated" : "deactivated";
        return ApiResponse<bool>.SuccessResponse(subject.IsActive, $"Subject has been {status}.");
    }
}
