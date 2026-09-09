using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Commands.ToggleBatchStatus;

public class ToggleBatchStatusCommandHandler : IRequestHandler<ToggleBatchStatusCommand, ApiResponse<bool>>
{
    private readonly IBatchRepository _batchRepository;
    private readonly ICurrentUser _currentUser;

    public ToggleBatchStatusCommandHandler(
        IBatchRepository batchRepository,
        ICurrentUser currentUser)
    {
        _batchRepository = batchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(ToggleBatchStatusCommand command, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (batch == null)
        {
            return ApiResponse<bool>.FailureResponse("Batch not found.");
        }

        batch.IsActive = !batch.IsActive;
        batch.UpdatedAt = DateTime.UtcNow;
        batch.UpdatedBy = _currentUser.UserId;

        _batchRepository.Update(batch);
        await _batchRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(batch.IsActive, $"Batch {(batch.IsActive ? "activated" : "deactivated")} successfully.");
    }
}
