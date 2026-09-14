using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.CreateFeeStructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.DeleteFeeStructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.ToggleFeeStructureStatus;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.UpdateFeeStructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetAllActiveFeeStructures;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructureById;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructuresPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.API.Controllers.BranchAdmin;

[Route("api/v1/branchadmin/fee-structures")]
[Authorize(Roles = "BranchAdmin,4")]
public class FeeStructuresController : BaseBranchAdminController
{
    public FeeStructuresController(
        IMediator mediator,
        ICurrentUser currentUser,
        IVargshalaDbContext db)
        : base(mediator, currentUser, db)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? classId = null,
        [FromQuery] string? session = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetFeeStructuresPagedQuery(request, branchId, classId, session, isActive), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var result = await Mediator.Send(new GetAllActiveFeeStructuresQuery(branchId, classId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var existsInBranch = await Db.FeeStructures.AsNoTracking()
            .AnyAsync(fs => fs.Id == id && !fs.IsDeleted && fs.BranchId == branchId, cancellationToken);
        if (!existsInBranch)
        {
            return NotFound(ApiResponse<FeeStructureDto>.FailureResponse("Fee structure not found in this branch."));
        }

        var result = await Mediator.Send(new GetFeeStructureByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeeStructureRequest request, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        // Strictly enforce current branch assignment
        request.BranchId = branchId;
        var result = await Mediator.Send(new CreateFeeStructureCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeeStructureRequest request, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        if (id != request.Id)
        {
            return BadRequest(ApiResponse<FeeStructureDto>.FailureResponse("Mismatched Fee Structure ID."));
        }

        var existsInBranch = await Db.FeeStructures.AsNoTracking()
            .AnyAsync(fs => fs.Id == id && !fs.IsDeleted && fs.BranchId == branchId, cancellationToken);
        if (!existsInBranch)
        {
            return NotFound(ApiResponse<FeeStructureDto>.FailureResponse("Fee structure not found in this branch."));
        }

        // Strictly lock to current branch
        request.BranchId = branchId;
        var result = await Mediator.Send(new UpdateFeeStructureCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var existsInBranch = await Db.FeeStructures.AsNoTracking()
            .AnyAsync(fs => fs.Id == id && !fs.IsDeleted && fs.BranchId == branchId, cancellationToken);
        if (!existsInBranch)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Fee structure not found in this branch."));
        }

        var result = await Mediator.Send(new DeleteFeeStructureCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken = default)
    {
        var (isValid, branchId, errorResult) = await ValidateBranchAccessAsync(cancellationToken);
        if (!isValid) return errorResult!;

        var existsInBranch = await Db.FeeStructures.AsNoTracking()
            .AnyAsync(fs => fs.Id == id && !fs.IsDeleted && fs.BranchId == branchId, cancellationToken);
        if (!existsInBranch)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Fee structure not found in this branch."));
        }

        var result = await Mediator.Send(new ToggleFeeStructureStatusCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
