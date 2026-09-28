namespace Vargshala.Contracts.Subscriptions;

/// <summary>
/// Request payload to switch or change an organization's subscription plan directly.
/// </summary>
public class ChangeSubscriptionPlanRequest
{
    public Guid TargetPlanId { get; set; }
    public string? Remarks { get; set; }
}
