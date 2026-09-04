using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.Users.Commands.CreateUser;
using Vargshala.Application.Features.Users.Queries.GetUsers;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Users;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Mobile,
            request.Password,
            request.Role);

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetUsers), result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] PagedRequest request)
    {
        var result = await _mediator.Send(new GetUsersQuery(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("controlpanel")]
    [Authorize(Roles = "SuperAdmin,BackOffice,1001,1002")]
    public async Task<IActionResult> GetControlPanelUsers(
        [FromQuery] PagedRequest request,
        [FromQuery] UserRole? role = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Users.Queries.GetControlPanelUsers.GetControlPanelUsersQuery(request, role, isActive));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("controlpanel")]
    [Authorize(Roles = "SuperAdmin,BackOffice,1001,1002")]
    public async Task<IActionResult> CreateControlPanelUser([FromBody] CreateUserRequest request)
    {
        var command = new Vargshala.Application.Features.Users.Commands.CreateControlPanelUser.CreateControlPanelUserCommand(
            request.FirstName,
            request.LastName,
            request.Email ?? string.Empty,
            request.Mobile,
            request.Password,
            request.Role,
            request.OrganizationId);

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("controlpanel/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,BackOffice,1001,1002")]
    public async Task<IActionResult> UpdateControlPanelUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await _mediator.Send(new Vargshala.Application.Features.Users.Commands.UpdateControlPanelUser.UpdateControlPanelUserCommand(request));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,BackOffice,1001,1002")]
    public async Task<IActionResult> ToggleUserStatus(Guid id)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Users.Commands.ToggleUserStatus.ToggleUserStatusCommand(id));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
