using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.ToggleFeeStructureStatus;

public class ToggleFeeStructureStatusCommandHandler : IRequestHandler<ToggleFeeStructureStatusCommand, ApiResponse<bool>>
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly ICurrentUser _currentUser;

    public ToggleFeeStructureStatusCommandHandler(
        IFeeStructureRepository feeStructureRepository,
        ICurrentUser currentUser)
    {
        _feeStructureRepository = feeStructureRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(ToggleFeeStructureStatusCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<bool>.FailureResponse("No active organization context found.");
        }

        var entity = await _feeStructureRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null || entity.OrganizationId != orgId.Value)
        {
            return ApiResponse<bool>.FailureResponse("Fee structure not found.");
        }

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _currentUser.UserId;

        _feeStructureRepository.Update(entity);
        await _feeStructureRepository.SaveChangesAsync(cancellationToken);

        var statusText = entity.IsActive ? "activated" : "deactivated";
        return ApiResponse<bool>.SuccessResponse(true, $"Fee structure {statusText} successfully.");
    }
}
