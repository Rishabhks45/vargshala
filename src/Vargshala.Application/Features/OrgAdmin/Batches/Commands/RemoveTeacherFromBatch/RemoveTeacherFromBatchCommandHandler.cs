using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.RemoveTeacherFromBatch;

public class RemoveTeacherFromBatchCommandHandler : IRequestHandler<RemoveTeacherFromBatchCommand, ApiResponse<bool>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICurrentUser _currentUser;

    public RemoveTeacherFromBatchCommandHandler(
        IBatchRepository batchRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(RemoveTeacherFromBatchCommand command, CancellationToken cancellationToken)
    {
        if (command.SubjectId.HasValue)
        {
            var existing = await _batchRepository.GetBatchTeacherAsync(command.BatchId, command.TeacherId, command.SubjectId.Value, cancellationToken);
            if (existing == null || !existing.IsActive)
            {
                return ApiResponse<bool>.FailureResponse("Teacher is not actively assigned to this subject in the batch.");
            }

            existing.IsActive = false;
            existing.RemovedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = _currentUser.UserId;

            _batchRepository.UpdateTeacher(existing);
            await _batchRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.SuccessResponse(true, "Teacher removed from batch subject successfully.");
        }
        else
        {
            var activeAssignments = await _batchRepository.GetBatchTeachersByTeacherAsync(command.BatchId, command.TeacherId, cancellationToken);
            if (activeAssignments == null || !activeAssignments.Any())
            {
                return ApiResponse<bool>.FailureResponse("Teacher is not actively assigned to this batch.");
            }

            foreach (var item in activeAssignments)
            {
                item.IsActive = false;
                item.RemovedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                item.UpdatedBy = _currentUser.UserId;
                _batchRepository.UpdateTeacher(item);
            }

            await _batchRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.SuccessResponse(true, "Teacher removed from batch successfully.");
        }
    }
}
