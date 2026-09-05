using Vargshala.Contracts.Common;
using Vargshala.Domain.Common;

namespace Vargshala.Domain.Entities;

public class Coupon : BaseEntity
{
    public Guid? OrganizationId { get; set; }

    public string Code { get; set; } = string.Empty;

    public CampaignCategory Category { get; set; } = CampaignCategory.Promotional;

    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public ApplicablePlan ApplicablePlan { get; set; } = ApplicablePlan.AllPlans;

    public int UsedCount { get; set; } = 0;

    public int MaxUses { get; set; } = 100;

    public DateTime ExpiryDate { get; set; }

    // Navigation
    public Organization? Organization { get; set; }
}
