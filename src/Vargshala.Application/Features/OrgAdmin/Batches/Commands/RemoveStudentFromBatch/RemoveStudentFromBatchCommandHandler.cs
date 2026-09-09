using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.RemoveStudentFromBatch;

public class RemoveStudentFromBatchCommandHandler : IRequestHandler<RemoveStudentFromBatchCommand, ApiResponse<bool>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICurrentUser _currentUser;

    public RemoveStudentFromBatchCommandHandler(
        IBatchRepository batchRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(RemoveStudentFromBatchCommand command, CancellationToken cancellationToken)
    {
        var existing = await _batchRepository.GetBatchStudentAsync(command.BatchId, command.StudentId, cancellationToken);
        if (existing == null || !existing.IsActive)
        {
            return ApiResponse<bool>.FailureResponse("Student is not actively enrolled in this batch.");
        }

        existing.IsActive = false;
        existing.LeftAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = _currentUser.UserId;

        _batchRepository.UpdateStudent(existing);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Student removed from batch successfully.");
    }
}
