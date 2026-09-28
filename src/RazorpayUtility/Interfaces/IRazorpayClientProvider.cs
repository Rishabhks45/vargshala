using Razorpay.Api;
using RazorpayUtility.Configuration;

namespace RazorpayUtility.Interfaces;

/// <summary>
/// Provider for obtaining authenticated instances of the official Razorpay .NET SDK client.
/// Automatically resolves API KeyId and KeySecret from database or appsettings.json.
/// </summary>
public interface IRazorpayClientProvider
{
    /// <summary>
    /// Gets an authenticated instance of the official RazorpayClient.
    /// </summary>
    Task<RazorpayClient> GetClientAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the resolved RazorpaySettings (KeyId, KeySecret, WebhookSecret, Currency, Branding).
    /// </summary>
    Task<RazorpaySettings> GetSettingsAsync(CancellationToken cancellationToken = default);
}
