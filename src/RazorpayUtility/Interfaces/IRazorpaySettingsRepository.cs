using RazorpayUtility.Configuration;

namespace RazorpayUtility.Interfaces;

/// <summary>
/// Interface for retrieving Razorpay configuration settings from the database.
/// </summary>
public interface IRazorpaySettingsRepository
{
    Task<RazorpaySettings?> GetSettingsAsync(CancellationToken cancellationToken = default);
}
