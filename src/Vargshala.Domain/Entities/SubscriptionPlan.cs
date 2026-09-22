using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

/// <summary>
/// Represents a SaaS pricing tier/package offered by Vargshala platform to coaching institutes.
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    /// <summary>
    /// Student capacity limit. NULL indicates unlimited students (e.g. Enterprise).
    /// </summary>
    public int? MaxStudents { get; set; }

    /// <summary>
    /// Teacher capacity limit. NULL indicates unlimited teachers.
    /// </summary>
    public int? MaxTeachers { get; set; }

    /// <summary>
    /// Branch capacity limit. NULL indicates unlimited branches.
    /// </summary>
    public int? MaxBranches { get; set; }

    // Navigation
    public ICollection<OrganizationSubscription> Subscriptions { get; set; } = new List<OrganizationSubscription>();
}
