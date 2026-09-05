using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Web.Services;

public interface ICouponService
{
    Task<ApiResponse<PagedResponse<CouponDto>>> GetCouponsPagedAsync(
        PagedRequest? request = null,
        CampaignCategory? category = null,
        DiscountType? discountType = null,
        ApplicablePlan? plan = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<CouponDto?> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<Guid>> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> UpdateCouponAsync(UpdateCouponRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteCouponAsync(Guid id, CancellationToken cancellationToken = default);
}
