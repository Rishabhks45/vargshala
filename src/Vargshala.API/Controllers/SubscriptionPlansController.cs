using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.SubscriptionPlans.Commands.CreateSubscriptionPlan;
using Vargshala.Application.Features.SubscriptionPlans.Commands.DeleteSubscriptionPlan;
using Vargshala.Application.Features.SubscriptionPlans.Commands.ToggleSubscriptionPlanStatus;
using Vargshala.Application.Features.SubscriptionPlans.Commands.UpdateSubscriptionPlan;
using Vargshala.Application.Features.SubscriptionPlans.Queries.GetActiveSubscriptionPlans;
using Vargshala.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Vargshala.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlansPaged;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/subscription-plans")]
public class SubscriptionPlansController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionPlansController> _logger;

    public SubscriptionPlansController(IMediator mediator, ILogger<SubscriptionPlansController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets active pricing plans (available for public pricing and organization subscription checkout).
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePlans(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetActiveSubscriptionPlansQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets paged list of SaaS subscription plans for platform control panel.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> GetPagedPlans(
        [FromQuery] PagedRequest request,
        [FromQuery] bool? isActive = null,
        [FromQuery] BillingCycle? billingCycle = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSubscriptionPlansPagedQuery(request, isActive, billingCycle), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single subscription plan by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubscriptionPlanByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Creates a new SaaS subscription pricing plan.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateSubscriptionPlanCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Updates an existing SaaS subscription plan.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            request.Id = id;
        }

        var result = await _mediator.Send(new UpdateSubscriptionPlanCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Toggles active status of a subscription plan.
    /// </summary>
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleSubscriptionPlanStatusCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a subscription plan.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,1001")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSubscriptionPlanCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
