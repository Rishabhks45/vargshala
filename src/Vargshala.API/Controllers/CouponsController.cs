using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.Coupons.Commands.CreateCoupon;
using Vargshala.Application.Features.Coupons.Commands.DeleteCoupon;
using Vargshala.Application.Features.Coupons.Commands.ToggleCouponStatus;
using Vargshala.Application.Features.Coupons.Commands.UpdateCoupon;
using Vargshala.Application.Features.Coupons.Queries.GetCouponByCode;
using Vargshala.Application.Features.Coupons.Queries.GetCouponById;
using Vargshala.Application.Features.Coupons.Queries.GetCoupons;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/coupons")]
[Authorize(Roles = "SuperAdmin,BackOffice,1001,1002")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CouponsController> _logger;

    public CouponsController(IMediator mediator, ILogger<CouponsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetCoupons(
        [FromQuery] PagedRequest request,
        [FromQuery] CampaignCategory? category,
        [FromQuery] DiscountType? discountType,
        [FromQuery] ApplicablePlan? plan,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCouponsQuery(request, category, discountType, plan, isActive), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCouponById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCouponByIdQuery(id), cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetCouponByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCouponByCodeQuery(code), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCouponCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetCouponById), new { id = result.Data }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] UpdateCouponRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Mismatched coupon ID in route and body."));
        }

        var result = await _mediator.Send(new UpdateCouponCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleCouponStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleCouponStatusCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCoupon(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCouponCommand(id), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
