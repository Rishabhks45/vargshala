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
}
