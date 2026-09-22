using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorpayUtility.Data;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Services;

namespace RazorpayUtility.Extensions;

/// <summary>
/// Dependency Injection extension methods for RazorpayUtility.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Razorpay services with PostgreSQL database-backed configuration.
    /// </summary>
    public static IServiceCollection AddRazorpayUtilityWithDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Database connection string is required for RazorpayUtility.", nameof(connectionString));
        }

        // 1. Register DbContext
        services.AddDbContext<RazorpayDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // 2. Register HttpClient for Razorpay API
        services.AddHttpClient();

        // 3. Register Core Services
        services.AddScoped<IRazorpaySettingsRepository, RazorpaySettingsRepository>();
        services.AddScoped<IRazorpayOrderService, RazorpayOrderService>();
        services.AddScoped<IRazorpayQrCodeService, RazorpayQrCodeService>();
        services.AddScoped<IRazorpayPaymentService, RazorpayPaymentService>();
        services.AddScoped<IRazorpayRefundService, RazorpayRefundService>();
        services.AddScoped<IRazorpayWebhookService, RazorpayWebhookService>();

        return services;
    }

    /// <summary>
    /// Registers all Razorpay services with PostgreSQL configuration and a strongly-typed Webhook Event Handler.
    /// </summary>
    public static IServiceCollection AddRazorpayUtilityWithDatabaseAndEventHandler<TEventHandler>(
        this IServiceCollection services,
        string connectionString)
        where TEventHandler : class, IRazorpayWebhookEventHandler
    {
        services.AddRazorpayUtilityWithDatabase(connectionString);
        services.AddScoped<IRazorpayWebhookEventHandler, TEventHandler>();
        return services;
    }
}