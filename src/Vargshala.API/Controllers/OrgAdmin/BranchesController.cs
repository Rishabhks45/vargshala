using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrgAdmin.Branches.Commands.AssignUserBranches;
using Vargshala.Application.Features.OrgAdmin.Branches.Commands.CreateBranch;
using Vargshala.Application.Features.OrgAdmin.Branches.Commands.DeleteBranch;
using Vargshala.Application.Features.OrgAdmin.Branches.Commands.UpdateBranch;
using Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetAllActiveBranches;
using Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetBranchById;
using Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetBranchesPaged;
using Vargshala.Application.Features.OrgAdmin.Branches.Queries.GetUserBranches;
using Vargshala.Contracts.Branches;
using Vargshala.Contracts.Common;

namespace Vargshala.API.Controllers.OrgAdmin;

[ApiController]
[Route("api/v1/orgadmin/branches")]
[Authorize(Roles = "OrganizationAdmin,1")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] PagedRequest request,
        [FromQuery] string? city = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetBranchesPagedQuery(request, city, isActive);
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("all-active")]
    public async Task<IActionResult> GetAllActive()
    {
        var result = await _mediator.Send(new GetAllActiveBranchesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBranchByIdQuery(id));
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request)
    {
        var result = await _mediator.Send(new CreateBranchCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<BranchDto>.FailureResponse("Mismatched Branch ID."));
        }

        var result = await _mediator.Send(new UpdateBranchCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteBranchCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserBranches(Guid userId)
    {
        var result = await _mediator.Send(new GetUserBranchesQuery(userId));
        return Ok(result);
    }

    [HttpPost("assign-user")]
    public async Task<IActionResult> AssignUserBranches([FromBody] AssignUserBranchesRequest request)
    {
        var result = await _mediator.Send(new AssignUserBranchesCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
