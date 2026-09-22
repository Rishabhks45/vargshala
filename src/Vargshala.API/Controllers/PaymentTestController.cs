using Microsoft.AspNetCore.Mvc;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;

namespace Vargshala.API.Controllers;

/// <summary>
/// Development & Testing Controller to verify Razorpay API connectivity and Dynamic QR Code generation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentTestController : ControllerBase
{
    private readonly IRazorpaySettingsRepository _settingsRepo;
    private readonly IRazorpayQrCodeService _qrCodeService;
    private readonly IRazorpayOrderService _orderService;

    public PaymentTestController(
        IRazorpaySettingsRepository settingsRepo,
        IRazorpayQrCodeService qrCodeService,
        IRazorpayOrderService orderService)
    {
        _settingsRepo = settingsRepo;
        _qrCodeService = qrCodeService;
        _orderService = orderService;
    }

    /// <summary>
    /// Checks if Razorpay credentials are properly loaded from the database or configuration.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepo.GetSettingsAsync(cancellationToken);
        if (settings == null || !settings.IsValid())
        {
            return Ok(new
            {
                isConfigured = false,
                message = "Razorpay credentials are not configured or are invalid."
            });
        }

        string maskedKey = settings.KeyId.Length > 8
            ? $"{settings.KeyId[..8]}...{settings.KeyId[^4..]}"
            : "configured";

        return Ok(new
        {
            isConfigured = true,
            keyId = maskedKey,
            currency = settings.Currency,
            companyName = settings.CompanyName,
            themeColor = settings.ThemeColor
        });
    }

    /// <summary>
    /// Generates a live test Dynamic UPI QR Code using Razorpay API.
    /// </summary>
    /// <param name="amount">Amount in Rupees (Default: ₹10)</param>
    [HttpPost("generate-test-qr")]
    public async Task<IActionResult> GenerateTestQr([FromQuery] decimal amount = 10.00m, CancellationToken cancellationToken = default)
    {
        var request = new RazorpayCreateQrCodeRequest
        {
            Amount = amount,
            Name = "Vargshala Test Fee",
            Description = $"Test Dynamic QR Payment of ₹{amount:N0}",
            Notes = new Dictionary<string, string>
            {
                { "test_source", "PaymentTestController" },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            },
            CloseBy = DateTime.UtcNow.AddMinutes(30)
        };

        var result = await _qrCodeService.CreateUpiQrCodeAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                isSuccess = false,
                errorMessage = result.ErrorMessage,
                errors = result.Errors
            });
        }

        return Ok(new
        {
            isSuccess = true,
            qrCodeId = result.QrCodeId,
            qrImageUrl = result.ImageUrl,
            amount = result.Amount,
            status = result.Status,
            expiresAt = result.CloseBy
        });
    }

    /// <summary>
    /// Creates a test Razorpay Order for standard Card / NetBanking / UPI checkout.
    /// </summary>
    /// <param name="amount">Amount in Rupees (Default: ₹50)</param>
    [HttpPost("create-test-order")]
    public async Task<IActionResult> CreateTestOrder([FromQuery] decimal amount = 50.00m, CancellationToken cancellationToken = default)
    {
        var request = new RazorpayCreateOrderRequest
        {
            Amount = amount,
            Receipt = $"test_{DateTime.UtcNow.Ticks % 1000000}",
            Notes = new Dictionary<string, string>
            {
                { "test_purpose", "Vargshala Order Verification" }
            }
        };

        var result = await _orderService.CreateOrderAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                isSuccess = false,
                errorMessage = result.ErrorMessage
            });
        }

        return Ok(new
        {
            isSuccess = true,
            orderId = result.OrderId,
            amount = result.Amount,
            amountPaise = result.AmountPaise,
            keyId = result.KeyId,
            currency = result.Currency,
            receipt = result.Receipt
        });
    }
}