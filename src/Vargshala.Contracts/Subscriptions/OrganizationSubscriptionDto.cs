using Vargshala.Contracts.Common;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Contracts.Subscriptions;

public class OrganizationSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;

    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal PlanPrice { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public string BillingCycleName => BillingCycle.GetDisplayName();

    public int? MaxStudents { get; set; }
    public string MaxStudentsDisplay => MaxStudents.HasValue ? MaxStudents.Value.ToString("N0") : "Unlimited";

    public int? MaxTeachers { get; set; }
    public string MaxTeachersDisplay => MaxTeachers.HasValue ? MaxTeachers.Value.ToString("N0") : "Unlimited";

    public int? MaxBranches { get; set; }
    public string MaxBranchesDisplay => MaxBranches.HasValue ? MaxBranches.Value.ToString("N0") : "Unlimited";

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public string StatusName => Status.GetDisplayName();

    public int RemainingDays => (int)Math.Max(0, (EndDate.Date - DateTime.UtcNow.Date).TotalDays);
    public bool IsActive => (Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Trial) && EndDate >= DateTime.UtcNow;
    public bool IsExpired => Status == SubscriptionStatus.Expired || EndDate < DateTime.UtcNow;

    public bool AutoRenew { get; set; }
    public DateTime CreatedAt { get; set; }
}
