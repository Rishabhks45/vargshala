using Microsoft.Extensions.Logging;
using Razorpay.Api;
using RazorpayUtility.Configuration;
using RazorpayUtility.Interfaces;

namespace RazorpayUtility.Services;

/// <summary>
/// Implementation of IRazorpayClientProvider that loads settings from PostgreSQL / appsettings.json
/// and instantiates the official RazorpayClient.
/// </summary>
public class RazorpayClientProvider : IRazorpayClientProvider
{
    private readonly IRazorpaySettingsRepository _settingsRepo;
    private readonly ILogger<RazorpayClientProvider> _logger;

    public RazorpayClientProvider(
        IRazorpaySettingsRepository settingsRepo,
        ILogger<RazorpayClientProvider> logger)
    {
        _settingsRepo = settingsRepo;
        _logger = logger;
    }

    public async Task<RazorpayClient> GetClientAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        return new RazorpayClient(settings.KeyId, settings.KeySecret);
    }

    public async Task<RazorpaySettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepo.GetSettingsAsync(cancellationToken);
        if (settings == null || !settings.IsValid())
        {
            _logger.LogError("Razorpay credentials (KeyId / KeySecret) are missing in the database.");
            throw new InvalidOperationException("Razorpay credentials are not configured. Please configure KeyId and KeySecret in the database.");
        }

        return settings;
    }
}
