namespace RazorpayUtility.Models;

/// <summary>
/// Base class for all Razorpay operation results providing error tracking.
/// </summary>
public abstract class RazorpayBaseResult
{
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
}

#region Orders

/// <summary>
/// Request model for creating a Razorpay Order.
/// </summary>
public class RazorpayCreateOrderRequest
{
    /// <summary>
    /// Amount in Rupees (e.g. 500.00). Will be converted to Paise automatically.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (Default: INR).
    /// </summary>
    public string Currency { get; set; } = "INR";

    /// <summary>
    /// Your internal receipt identifier (e.g., student receipt number, subscription invoice ID). Max 40 chars.
    /// </summary>
    public string Receipt { get; set; } = string.Empty;

    /// <summary>
    /// Custom key-value pairs (e.g. OrganizationId, StudentId, InstallmentId).
    /// </summary>
    public Dictionary<string, string>? Notes { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
}

/// <summary>
/// Response model for a created Razorpay Order.
/// </summary>
public class RazorpayOrderResponse : RazorpayBaseResult
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public long AmountPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public string Receipt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "created", "attempted", "paid"
    public string KeyId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = "Vargshala";
    public string ThemeColor { get; set; } = "#009488";
    public Dictionary<string, string>? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Dynamic UPI QR Codes

/// <summary>
/// Request model for creating a Dynamic Single-Use UPI QR Code.
/// </summary>
public class RazorpayCreateQrCodeRequest
{
    /// <summary>
    /// Exact amount in Rupees (e.g. 2500.00).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Name displayed on the QR Code (e.g., "Vargshala Student Fee").
    /// </summary>
    public string Name { get; set; } = "Vargshala Payment";

    /// <summary>
    /// Description of the payment (e.g., "Term 1 Installment").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional customer information.
    /// </summary>
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Custom tracking notes (OrganizationId, StudentId, etc.).
    /// </summary>
    public Dictionary<string, string>? Notes { get; set; }

    /// <summary>
    /// Time after which the QR code expires (Default: 30 minutes).
    /// </summary>
    public DateTime? CloseBy { get; set; }
}

/// <summary>
/// Response model containing the Dynamic UPI QR Code image and metadata.
/// </summary>
public class RazorpayQrCodeResponse : RazorpayBaseResult
{
    public string QrCodeId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty; // Direct PNG URL of the QR Code
    public string Status { get; set; } = string.Empty;   // "active", "closed"
    public decimal Amount { get; set; }
    public decimal PaymentsAmountReceived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CloseBy { get; set; }
    public Dictionary<string, string>? Notes { get; set; }
}

#endregion

#region Payment Verification & Details

/// <summary>
/// Request to verify signature returned by Razorpay Standard Checkout modal.
/// </summary>
public class RazorpayPaymentVerifyRequest
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}

/// <summary>
/// Result of payment signature verification and fetched status.
/// </summary>
public class RazorpayPaymentVerifyResult : RazorpayBaseResult
{
    public bool IsValidSignature { get; set; }
    public string PaymentId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // "captured", "authorized", "failed"
    public string Method { get; set; } = string.Empty; // "card", "upi", "netbanking", "wallet"
    public string? Vpa { get; set; }                   // UPI VPA (e.g. user@okhdfcbank)
    public string? Bank { get; set; }
    public string? Wallet { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardNetwork { get; set; }           // "Visa", "MasterCard", "RuPay"
    public string? Email { get; set; }
    public string? Contact { get; set; }
    public Dictionary<string, string>? Notes { get; set; }
}

/// <summary>
/// Full payment details retrieved from Razorpay API.
/// </summary>
public class RazorpayPaymentDetails : RazorpayBaseResult
{
    public string Id { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = string.Empty; // "captured", "authorized", "refunded", "failed"
    public string Method { get; set; } = string.Empty; // "card", "upi", "netbanking"
    public string? Description { get; set; }
    public string? Vpa { get; set; }
    public string? Bank { get; set; }
    public string? Wallet { get; set; }
    public string? Email { get; set; }
    public string? Contact { get; set; }
    public decimal? Fee { get; set; }
    public decimal? Tax { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDescription { get; set; }
    public Dictionary<string, string>? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Refunds

/// <summary>
/// Request to initiate full or partial refund.
/// </summary>
public class RazorpayRefundRequest
{
    public string PaymentId { get; set; } = string.Empty;
    /// <summary>
    /// Amount in Rupees. If null or 0, full payment is refunded.
    /// </summary>
    public decimal? Amount { get; set; }
    public Dictionary<string, string>? Notes { get; set; }
}

/// <summary>
/// Result of a refund operation.
/// </summary>
public class RazorpayRefundResponse : RazorpayBaseResult
{
    public string RefundId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = string.Empty; // "processed", "pending", "failed"
    public DateTime CreatedAt { get; set; }
}

#endregion
