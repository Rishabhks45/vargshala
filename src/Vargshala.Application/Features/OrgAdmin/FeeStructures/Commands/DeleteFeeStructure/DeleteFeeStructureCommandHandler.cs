using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.DeleteFeeStructure;

public class DeleteFeeStructureCommandHandler : IRequestHandler<DeleteFeeStructureCommand, ApiResponse<bool>>
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly ICurrentUser _currentUser;

    public DeleteFeeStructureCommandHandler(
        IFeeStructureRepository feeStructureRepository,
        ICurrentUser currentUser)
    {
        _feeStructureRepository = feeStructureRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteFeeStructureCommand command, CancellationToken cancellationToken)
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

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = _currentUser.UserId;

        _feeStructureRepository.Update(entity);
        await _feeStructureRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Fee structure deleted successfully.");
    }
}
