using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.ToggleClassStatus;

public class ToggleClassStatusCommandHandler : IRequestHandler<ToggleClassStatusCommand, ApiResponse<bool>>
{
    private readonly IClassRepository _classRepository;
    private readonly ICurrentUser _currentUser;

    public ToggleClassStatusCommandHandler(
        IClassRepository classRepository,
        ICurrentUser currentUser)
    {
        _classRepository = classRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(ToggleClassStatusCommand command, CancellationToken cancellationToken)
    {
        var entity = await _classRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);
        if (entity == null)
        {
            return ApiResponse<bool>.FailureResponse("Class not found.");
        }

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _currentUser.UserId;

        _classRepository.Update(entity);
        await _classRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(entity.IsActive, $"Class {(entity.IsActive ? "activated" : "deactivated")} successfully.");
    }
}
