using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Coupons.Infrastructure;

#region Interface
public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Coupon?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(List<Coupon> Items, int TotalRecords)> GetCouponsPagedAsync(
        PagedRequest request,
        CampaignCategory? category = null,
        DiscountType? discountType = null,
        ApplicablePlan? plan = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default);
    void Update(Coupon coupon);
    void Delete(Coupon coupon);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
#endregion
