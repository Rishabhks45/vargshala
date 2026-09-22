using Vargshala.Contracts.Subscriptions;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.SubscriptionPlans;

public static class SubscriptionPlanMappingExtensions
{
    public static SubscriptionPlanDto ToDto(this SubscriptionPlan plan, int? activeInstitutesCount = null)
    {
        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            BillingCycle = plan.BillingCycle,
            MaxStudents = plan.MaxStudents,
            MaxTeachers = plan.MaxTeachers,
            MaxBranches = plan.MaxBranches,
            IsActive = plan.IsActive,
            ActiveInstitutesCount = activeInstitutesCount ?? plan.Subscriptions?.Count(s => s.Status == Vargshala.SharedKernel.Enums.SubscriptionStatus.Active && !s.IsDeleted) ?? 0,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt
        };
    }
}
