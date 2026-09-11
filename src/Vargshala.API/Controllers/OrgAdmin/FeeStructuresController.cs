using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.CreateFeeStructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.DeleteFeeStructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.ToggleFeeStructureStatus;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Commands.UpdateFeeStructure;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetAllActiveFeeStructures;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructureById;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Queries.GetFeeStructuresPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.FeeStructures;

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/fee-structures")]
[Authorize(Roles = "OrganizationAdmin,1,SuperAdmin,1001")]
public class FeeStructuresController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeeStructuresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? classId = null,
        [FromQuery] string? session = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new GetFeeStructuresPagedQuery(request, branchId, classId, session, isActive));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? classId = null)
    {
        var result = await _mediator.Send(new GetAllActiveFeeStructuresQuery(branchId, classId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetFeeStructureByIdQuery(id));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeeStructureRequest request)
    {
        var result = await _mediator.Send(new CreateFeeStructureCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeeStructureRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<FeeStructureDto>.FailureResponse("Mismatched Fee Structure ID."));
        }

        var result = await _mediator.Send(new UpdateFeeStructureCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteFeeStructureCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var result = await _mediator.Send(new ToggleFeeStructureStatusCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
