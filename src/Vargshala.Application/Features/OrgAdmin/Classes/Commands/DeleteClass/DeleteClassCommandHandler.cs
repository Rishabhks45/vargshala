using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.DeleteClass;

public class DeleteClassCommandHandler : IRequestHandler<DeleteClassCommand, ApiResponse<bool>>
{
    private readonly IClassRepository _classRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteClassCommandHandler(
        IClassRepository classRepository,
        ICurrentUser currentUser)
    {
        _classRepository = classRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteClassCommand command, CancellationToken cancellationToken)
    {
        var entity = await _classRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (entity == null)
        {
            return ApiResponse<bool>.FailureResponse("Class not found.");
        }

        var hasBatches = await _classRepository.HasBatchesAsync(command.Id, cancellationToken);
        if (hasBatches)
        {
            return ApiResponse<bool>.FailureResponse("Cannot delete class because active batches are associated with it. Please delete or reassign batches first.");
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = _currentUser.UserId;

        _classRepository.Delete(entity);
        await _classRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Class deleted successfully.");
    }
}
