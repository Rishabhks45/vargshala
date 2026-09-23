using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.OrganizationSubscriptions.Queries.GetSubscriptionPaymentReceiptPdf;
using Vargshala.Application.Features.Payments.Queries.GetPaymentLogsPaged;
using Vargshala.Application.Features.Payments.Queries.GetPaymentLogsStats;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Payments;
using Vargshala.Domain.Entities;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/controlpanel/payments")]
[Authorize(Roles = "SuperAdmin,1001")]
public class PaymentLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentLogsController> _logger;

    public PaymentLogsController(
        IMediator mediator,
        ILogger<PaymentLogsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets paged payment gateway transactions and audit logs across all institutes.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaymentLogs(
        [FromQuery] PagedRequest request,
        [FromQuery] string? method,
        [FromQuery] string? status,
        [FromQuery] PaymentType? paymentType,
        CancellationToken cancellationToken)
    {
        var query = new GetPaymentLogsPagedQuery(request, method, status, paymentType);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets high-level summary KPIs (30d volume, success rate, failed transactions count) for the audit dashboard.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetPaymentStats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentLogsStatsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Downloads the official QuestPDF generated tax invoice / receipt for any payment.
    /// </summary>
    [HttpGet("{paymentId:guid}/receipt/pdf")]
    public async Task<IActionResult> GetReceiptPdf(Guid paymentId, CancellationToken cancellationToken)
    {
        var query = new GetSubscriptionPaymentReceiptPdfQuery(paymentId);
        var result = await _mediator.Send(query, cancellationToken);
        if (!result.Success || result.Data == null)
        {
            return NotFound(result);
        }
        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }
}
