namespace Vargshala.Contracts.Subscriptions;

/// <summary>
/// Request payload to cancel the caller organization's active SaaS subscription.
/// </summary>
public class CancelSubscriptionRequest
{
    public string? Reason { get; set; }
}
