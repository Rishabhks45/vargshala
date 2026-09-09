using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Classes;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Commands.CreateClass;

public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, ApiResponse<ClassDto>>
{
    private readonly IClassRepository _classRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUser _currentUser;

    public CreateClassCommandHandler(
        IClassRepository classRepository,
        IBranchRepository branchRepository,
        ICurrentUser currentUser)
    {
        _classRepository = classRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ClassDto>> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<ClassDto>.FailureResponse("No active organization context found.");
        }

        var req = command.Request;

        // Verify Branch belongs to Current Organization
        var branch = await _branchRepository.GetByIdAsync(req.BranchId, cancellationToken);
        if (branch == null || branch.OrganizationId != orgId.Value)
        {
            return ApiResponse<ClassDto>.FailureResponse("Branch not found in your organization.");
        }

        // Unique (BranchId, Code)
        var exists = await _classRepository.ExistsByCodeAndBranchAsync(req.Code.Trim(), req.BranchId, cancellationToken: cancellationToken);
        if (exists)
        {
            return ApiResponse<ClassDto>.FailureResponse($"A class with code '{req.Code.Trim()}' already exists in this branch.");
        }

        var entity = new Class
        {
            Id = Guid.NewGuid(),
            BranchId = req.BranchId,
            Name = req.Name.Trim(),
            Code = req.Code.Trim().ToUpperInvariant(),
            Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _classRepository.AddAsync(entity, cancellationToken);
        await _classRepository.SaveChangesAsync(cancellationToken);

        entity.Branch = branch;
        return ApiResponse<ClassDto>.SuccessResponse(entity.ToDto(), "Class created successfully.");
    }
}
