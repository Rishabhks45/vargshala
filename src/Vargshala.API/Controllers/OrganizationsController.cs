using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.Organizations.Queries.GetOrganization;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyOrganization()
    {
        var result = await _mediator.Send(new GetOrganizationQuery());

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> GetAllInstitutes()
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Organizations.Queries.GetAllInstitutes.GetAllInstitutesQuery());
        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Organizations.Commands.ToggleStatus.ToggleOrganizationStatusCommand(id));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> UpdateInstitute(Guid id, [FromBody] Vargshala.Contracts.Organizations.UpdateOrganizationRequest request)
    {
        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await _mediator.Send(new Vargshala.Application.Features.Organizations.Commands.UpdateOrganization.UpdateOrganizationCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
