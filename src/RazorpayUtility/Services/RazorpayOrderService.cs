using Microsoft.Extensions.Logging;
using Razorpay.Api;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;

namespace RazorpayUtility.Services;

/// <summary>
/// Service for Razorpay Order creation and status tracking using the official Razorpay SDK.
/// </summary>
public class RazorpayOrderService : IRazorpayOrderService
{
    private readonly IRazorpayClientProvider _clientProvider;
    private readonly ILogger<RazorpayOrderService> _logger;

    public RazorpayOrderService(
        IRazorpayClientProvider clientProvider,
        ILogger<RazorpayOrderService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<RazorpayOrderResponse> CreateOrderAsync(RazorpayCreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        long amountPaise = (long)Math.Round(request.Amount * 100m);
        if (amountPaise <= 0)
        {
            return new RazorpayOrderResponse
            {
                IsSuccess = false,
                ErrorMessage = "Order amount must be greater than 0."
            };
        }

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        var settings = await _clientProvider.GetSettingsAsync(cancellationToken);

        var options = new Dictionary<string, object>
        {
            { "amount", amountPaise },
            { "currency", string.IsNullOrWhiteSpace(request.Currency) ? settings.Currency : request.Currency },
            { "receipt", string.IsNullOrWhiteSpace(request.Receipt) ? $"rcpt_{Guid.NewGuid():N}"[..20] : request.Receipt }
        };

        if (request.Notes != null && request.Notes.Count > 0)
        {
            options["notes"] = request.Notes;
        }

        Order order = client.Order.Create(options);
        if (order == null)
        {
            return new RazorpayOrderResponse
            {
                IsSuccess = false,
                ErrorMessage = "Failed to create order via Razorpay SDK."
            };
        }

        long createdAmountPaise = order["amount"] != null ? Convert.ToInt64(order["amount"]) : amountPaise;

        return new RazorpayOrderResponse
        {
            IsSuccess = true,
            OrderId = order["id"]?.ToString() ?? string.Empty,
            Amount = request.Amount,
            AmountPaise = createdAmountPaise,
            Currency = order["currency"]?.ToString() ?? settings.Currency,
            Receipt = order["receipt"]?.ToString() ?? string.Empty,
            Status = order["status"]?.ToString() ?? "created",
            KeyId = settings.KeyId,
            CompanyName = settings.CompanyName,
            ThemeColor = settings.ThemeColor,
            Notes = request.Notes
        };
    }

    public async Task<RazorpayOrderResponse?> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return null;

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        var settings = await _clientProvider.GetSettingsAsync(cancellationToken);

        Order order = client.Order.Fetch(orderId);
        if (order == null)
            return null;

        long amountPaise = order["amount"] != null ? Convert.ToInt64(order["amount"]) : 0;
        return new RazorpayOrderResponse
        {
            OrderId = order["id"]?.ToString() ?? orderId,
            Amount = amountPaise / 100m,
            AmountPaise = amountPaise,
            Currency = order["currency"]?.ToString() ?? "INR",
            Receipt = order["receipt"]?.ToString() ?? string.Empty,
            Status = order["status"]?.ToString() ?? string.Empty,
            KeyId = settings.KeyId,
            CompanyName = settings.CompanyName,
            ThemeColor = settings.ThemeColor
        };
    }

    public async Task<List<RazorpayPaymentDetails>> GetOrderPaymentsAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var payments = new List<RazorpayPaymentDetails>();
        if (string.IsNullOrWhiteSpace(orderId))
            return payments;

        var client = await _clientProvider.GetClientAsync(cancellationToken);
        Order order = client.Order.Fetch(orderId);
        if (order == null)
            return payments;

        List<Payment> rzpPayments = order.Payments();
        if (rzpPayments == null)
            return payments;

        foreach (Payment item in rzpPayments)
        {
            long amtPaise = item["amount"] != null ? Convert.ToInt64(item["amount"]) : 0;
            payments.Add(new RazorpayPaymentDetails
            {
                Id = item["id"]?.ToString() ?? string.Empty,
                OrderId = item["order_id"]?.ToString(),
                Amount = amtPaise / 100m,
                Currency = item["currency"]?.ToString() ?? "INR",
                Status = item["status"]?.ToString() ?? string.Empty,
                Method = item["method"]?.ToString() ?? string.Empty,
                Vpa = item["vpa"]?.ToString(),
                Bank = item["bank"]?.ToString(),
                Wallet = item["wallet"]?.ToString(),
                Email = item["email"]?.ToString(),
                Contact = item["contact"]?.ToString()
            });
        }

        return payments;
    }
}
