using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.DeleteBatch;

public class DeleteBatchCommandHandler : IRequestHandler<DeleteBatchCommand, ApiResponse<bool>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteBatchCommandHandler(
        IBatchRepository batchRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteBatchCommand command, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (batch == null)
        {
            return ApiResponse<bool>.FailureResponse("Batch not found.");
        }

        batch.IsDeleted = true;
        batch.DeletedAt = DateTime.UtcNow;
        batch.DeletedBy = _currentUser.UserId;

        _batchRepository.Delete(batch);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Batch deleted successfully.");
    }
}
