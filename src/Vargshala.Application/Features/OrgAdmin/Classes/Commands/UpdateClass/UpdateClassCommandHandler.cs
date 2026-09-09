using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.UpdateClass;

public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, ApiResponse<ClassDto>>
{
    private readonly IClassRepository _classRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateClassCommandHandler(
        IClassRepository classRepository,
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _classRepository = classRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ClassDto>> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<ClassDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        var entity = await _classRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (entity == null)
        {
            return ApiResponse<ClassDto>.FailureResponse("Class not found.");
        }

        // Verify Branch belongs to Current Organization
        var branch = await _branchRepository.GetByIdAsync(req.BranchId, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<ClassDto>.FailureResponse("Branch not found in your organization.");
        }

        // Unique (BranchId, Code)
        var exists = await _classRepository.ExistsByCodeAndBranchAsync(req.Code.Trim(), req.BranchId, excludeId: req.Id, cancellationToken: cancellationToken);
        if (exists)
        {
            return ApiResponse<ClassDto>.FailureResponse($"A class with code '{req.Code.Trim()}' already exists in this branch.");
        }

        entity.BranchId = req.BranchId;
        entity.Name = req.Name.Trim();
        entity.Code = req.Code.Trim().ToUpperInvariant();
        entity.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _currentUser.UserId;

        _classRepository.Update(entity);
        await _classRepository.SaveChangesAsync(cancellationToken);

        entity.Branch = branch;
        return ApiResponse<ClassDto>.SuccessResponse(entity.ToDto(), "Class updated successfully.");
    }
}
