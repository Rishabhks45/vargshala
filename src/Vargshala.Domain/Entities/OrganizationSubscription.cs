using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

/// <summary>
/// Represents an active or historical SaaS subscription purchased by an educational organization.
/// </summary>
public class OrganizationSubscription : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid PlanId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Subscription status: Trial, Active, Expired, Cancelled, Suspended
    /// </summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

    public bool AutoRenew { get; set; } = false;

    // Navigation
    public Organization Organization { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
