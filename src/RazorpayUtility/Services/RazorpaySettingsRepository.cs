using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RazorpayUtility.Configuration;
using RazorpayUtility.Data;
using RazorpayUtility.Interfaces;

namespace RazorpayUtility.Services;

/// <summary>
/// Repository for fetching Razorpay settings from PostgreSQL, with fallback to appsettings.json.
/// </summary>
public class RazorpaySettingsRepository : IRazorpaySettingsRepository
{
    private readonly RazorpayDbContext _context;
    private readonly IConfiguration? _configuration;

    public RazorpaySettingsRepository(RazorpayDbContext context, IConfiguration? configuration = null)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<RazorpaySettings?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            RazorpaySettingsEntity? entity = await _context.RazorpaySettings
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null && !string.IsNullOrWhiteSpace(entity.KeyId))
            {
                return new RazorpaySettings
                {
                    KeyId = entity.KeyId,
                    KeySecret = entity.KeySecret,
                    WebhookSecret = entity.WebhookSecret,
                    Currency = entity.Currency,
                    CompanyName = entity.CompanyName,
                    ThemeColor = entity.ThemeColor,
                    SuccessUrl = entity.SuccessUrl,
                    CancelUrl = entity.CancelUrl
                };
            }
        }
        catch
        {
            // Table might not exist yet or DB connection issue; proceed to fallback configuration
        }

        // Fallback: Read from appsettings.json "Razorpay" section
        if (_configuration != null)
        {
            string? keyId = _configuration["Razorpay:KeyId"];
            string? keySecret = _configuration["Razorpay:KeySecret"];

            if (!string.IsNullOrWhiteSpace(keyId) && !string.IsNullOrWhiteSpace(keySecret))
            {
                return new RazorpaySettings
                {
                    KeyId = keyId,
                    KeySecret = keySecret,
                    WebhookSecret = _configuration["Razorpay:WebhookSecret"] ?? string.Empty,
                    Currency = _configuration["Razorpay:Currency"] ?? "INR",
                    CompanyName = _configuration["Razorpay:CompanyName"] ?? "Vargshala",
                    ThemeColor = _configuration["Razorpay:ThemeColor"] ?? "#009488",
                    SuccessUrl = _configuration["Razorpay:SuccessUrl"] ?? string.Empty,
                    CancelUrl = _configuration["Razorpay:CancelUrl"] ?? string.Empty
                };
            }
        }

        return null;
    }
}