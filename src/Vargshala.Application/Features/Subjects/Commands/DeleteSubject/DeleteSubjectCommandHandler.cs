using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Subjects.Commands.DeleteSubject;

public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, ApiResponse<bool>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteSubjectCommandHandler(
        ISubjectRepository subjectRepository,
        ICurrentUser currentUser)
    {
        _subjectRepository = subjectRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteSubjectCommand command,
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

        subject.IsDeleted = true;
        subject.DeletedAt = DateTime.UtcNow;
        subject.DeletedBy = _currentUser.UserId;

        _subjectRepository.Update(subject);
        await _subjectRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Subject deleted successfully.");
    }
}
