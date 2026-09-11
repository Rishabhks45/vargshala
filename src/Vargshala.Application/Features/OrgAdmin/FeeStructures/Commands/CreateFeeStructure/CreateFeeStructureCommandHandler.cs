using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.CreateFeeStructure;

public class CreateFeeStructureCommandHandler : IRequestHandler<CreateFeeStructureCommand, ApiResponse<FeeStructureDto>>
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IClassRepository _classRepository;
    private readonly ICurrentUser _currentUser;

    public CreateFeeStructureCommandHandler(
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

    public async Task<ApiResponse<FeeStructureDto>> Handle(CreateFeeStructureCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        // Verify Branch belongs to Current Organization
        var branch = await _branchRepository.GetByIdAsync(req.BranchId, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse("Selected branch not found in your organization.");
        }

        // Verify Class if specified
        Class? cls = null;
        if (req.ClassId.HasValue && req.ClassId.Value != Guid.Empty)
        {
            cls = await _classRepository.GetByIdAsync(req.ClassId.Value, cancellationToken);
            if (cls == null || cls.BranchId != req.BranchId)
            {
                return ApiResponse<FeeStructureDto>.FailureResponse("Selected class does not belong to the chosen branch.");
            }
        }

        // Check Unique (Name, Branch, Session)
        var exists = await _feeStructureRepository.ExistsByNameAndBranchAsync(
            req.Name.Trim(),
            req.BranchId,
            req.AcademicSession.Trim(),
            cancellationToken: cancellationToken);

        if (exists)
        {
            return ApiResponse<FeeStructureDto>.FailureResponse($"A fee structure with name '{req.Name.Trim()}' already exists for this branch and academic session.");
        }

        var entity = new FeeStructure
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId.Value,
            BranchId = req.BranchId,
            ClassId = req.ClassId.HasValue && req.ClassId.Value != Guid.Empty ? req.ClassId : null,
            Name = req.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
            TotalAmount = req.TotalAmount,
            AcademicSession = req.AcademicSession.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _feeStructureRepository.AddAsync(entity, cancellationToken);
        await _feeStructureRepository.SaveChangesAsync(cancellationToken);

        entity.Branch = branch;
        entity.Class = cls;

        return ApiResponse<FeeStructureDto>.SuccessResponse(entity.ToDto(), "Fee structure created successfully.");
    }
}
