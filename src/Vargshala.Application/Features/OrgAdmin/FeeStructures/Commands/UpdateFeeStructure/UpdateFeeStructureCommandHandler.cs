using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.UpdateFeeStructure;

public class UpdateFeeStructureCommandHandler : IRequestHandler<UpdateFeeStructureCommand, ApiResponse<FeeStructureDto>>
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IClassRepository _classRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateFeeStructureCommandHandler(
        IFeeStructureRepository feeStructureRepository,
        IBranchRepository branchRepository,
        IClassRepository classRepository,
        ICurrentUser currentUser)
    {
        _feeStructureRepository = feeStructureRepository;
        _branchRepository = branchRepository;
        _classRepository = classRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<FeeStructureDto>> Handle(UpdateFeeStructureCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        var entity = await _feeStructureRepository.GetByIdAsync(req.Id, cancellationToken);
        if (entity == null || entity.OrganizationId != orgId.Value)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse("Fee structure not found.");
        }

        // Verify Branch
        var branch = await _branchRepository.GetByIdAsync(req.BranchId, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse("Selected branch not found in your organization.");
        }

        // Verify Class if provided
        Class? cls = null;
        if (req.ClassId.HasValue && req.ClassId.Value != Guid.Empty)
        {
            cls = await _classRepository.GetByIdAsync(req.ClassId.Value, cancellationToken);
            if (cls == null || cls.BranchId != req.BranchId)
            {
                return ApiResponse<FeeStructureDto>.FailureResponse("Selected class does not belong to the chosen branch.");
            }
        }

        // Check Unique (Name, Branch, Session) excluding this entity
        var exists = await _feeStructureRepository.ExistsByNameAndBranchAsync(
            req.Name.Trim(),
            req.BranchId,
            req.AcademicSession.Trim(),
            excludeId: req.Id,
            cancellationToken: cancellationToken);

        if (exists)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse($"Another fee structure with name '{req.Name.Trim()}' already exists for this branch and academic session.");
        }

        entity.BranchId = req.BranchId;
        entity.ClassId = req.ClassId.HasValue && req.ClassId.Value != Guid.Empty ? req.ClassId : null;
        entity.Name = req.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        entity.TotalAmount = req.TotalAmount;
        entity.AcademicSession = req.AcademicSession.Trim();
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _currentUser.UserId;

        _feeStructureRepository.Update(entity);
        await _feeStructureRepository.SaveChangesAsync(cancellationToken);

        entity.Branch = branch;
        entity.Class = cls;

        return ApiResponse<FeeStructureDto>.SuccessResponse(entity.ToDto(), "Fee structure updated successfully.");
    }
}
