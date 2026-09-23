using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrganizationSubscriptions.Commands.ConfirmSubscriptionPayment;
using Vargshala.Application.Features.OrganizationSubscriptions.Commands.CreateSubscriptionOrder;
using Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetCurrentSubscription;
using Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetSubscriptionHistory;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/organization-subscriptions")]
[Authorize]
public class OrganizationSubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrganizationSubscriptionsController> _logger;

    public OrganizationSubscriptionsController(
        IMediator mediator,
        ILogger<OrganizationSubscriptionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets current active SaaS subscription and real-time quota usage for the caller's educational organization.
    /// </summary>
    [HttpGet("current")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1001,1002")]
    public async Task<IActionResult> GetCurrentSubscription(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentSubscriptionQuery(), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Gets past subscription billing transactions and invoices for the caller's organization.
    /// </summary>
    [HttpGet("history")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1001,1002")]
    public async Task<IActionResult> GetSubscriptionHistory(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubscriptionHistoryQuery(), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Initializes checkout for a subscription plan, validates coupon discount, and creates a Razorpay Order.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1001,1002")]
    public async Task<IActionResult> Checkout(
        [FromBody] SubscriptionCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateSubscriptionOrderCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Verifies cryptographic payment signature from Razorpay and activates/extends the SaaS subscription.
    /// </summary>
    [HttpPost("confirm")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1001,1002")]
    public async Task<IActionResult> ConfirmPayment(
        [FromBody] ConfirmSubscriptionPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmSubscriptionPaymentCommand(request), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Gets the structured official receipt data for a subscription payment.
    /// </summary>
    [HttpGet("payments/{paymentId:guid}/receipt")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1001,1002")]
    public async Task<IActionResult> GetReceipt(Guid paymentId, CancellationToken cancellationToken)
    {
        var query = new Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetSubscriptionPaymentReceipt.GetSubscriptionPaymentReceiptQuery(paymentId);
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Downloads the official QuestPDF generated PDF tax invoice / receipt for a subscription payment.
    /// </summary>
    [HttpGet("payments/{paymentId:guid}/receipt/pdf")]
    [Authorize(Roles = "OrganizationAdmin,SuperAdmin,1001,1002")]
    public async Task<IActionResult> GetReceiptPdf(Guid paymentId, CancellationToken cancellationToken)
    {
        var query = new Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetSubscriptionPaymentReceiptPdf.GetSubscriptionPaymentReceiptPdfQuery(paymentId);
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }
        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }
}
