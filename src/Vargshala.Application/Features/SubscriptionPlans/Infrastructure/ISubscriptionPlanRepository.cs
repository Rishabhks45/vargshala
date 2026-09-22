using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Application.Features.SubscriptionPlans.Infrastructure;

public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<List<SubscriptionPlan>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<(List<SubscriptionPlan> Items, int TotalRecords)> GetPagedAsync(
        PagedRequest request,
        bool? isActive = null,
        BillingCycle? billingCycle = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);
    void Update(SubscriptionPlan plan);
    void Delete(SubscriptionPlan plan);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
