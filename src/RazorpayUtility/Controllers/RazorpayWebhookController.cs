using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RazorpayUtility.Interfaces;

namespace RazorpayUtility.Controllers;

/// <summary>
/// Controller for receiving and processing asynchronous Razorpay webhook events.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RazorpayWebhookController : ControllerBase
{
    private readonly IRazorpayWebhookService _webhookService;
    private readonly ILogger<RazorpayWebhookController> _logger;

    public RazorpayWebhookController(
        IRazorpayWebhookService webhookService,
        ILogger<RazorpayWebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming webhook payloads from Razorpay servers.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        try
        {
            string signature = Request.Headers["X-Razorpay-Signature"].ToString();
            using StreamReader reader = new(Request.Body);
            string jsonPayload = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                return BadRequest("Empty webhook body payload.");
            }

            bool processed = await _webhookService.ProcessWebhookAsync(jsonPayload, signature, cancellationToken);
            if (!processed)
            {
                return BadRequest("Webhook processing or signature verification failed.");
            }

            return Ok(new { status = "acknowledged" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing Razorpay webhook: {Message}", ex.Message);
            return StatusCode(500, "Internal error processing webhook.");
        }
    }
}