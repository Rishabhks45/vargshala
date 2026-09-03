using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.Authentication.Commands.Login;
using Vargshala.Application.Features.Authentication.Commands.RefreshToken;
using Vargshala.Application.Features.Authentication.Commands.RegisterOrganization;
using Vargshala.Application.Features.Authentication.Commands.RegisterUser;
using Vargshala.Contracts.Authentication;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [HttpPost("register-organization")]
    public async Task<IActionResult> RegisterOrganization([FromBody] RegisterOrganizationRequest request)
    {
        var command = new RegisterOrganizationCommand(
            request.OrganizationName,
            request.OrganizationCode,
            request.LogoUrl,
            request.Email,
            request.Mobile,
            request.Address,
            request.City,
            request.State,
            request.Pincode,
            request.AcademicSession,
            request.AdminFirstName,
            request.AdminLastName,
            request.AdminEmail,
            request.AdminMobile,
            request.Password);

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(
            request.OrganizationCode,
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

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
