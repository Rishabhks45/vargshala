using MediatR;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, ApiResponse<bool>>
{
    private readonly ICouponRepository _couponRepository;

    public UpdateCouponCommandHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<bool>> Handle(
        UpdateCouponCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var coupon = await _couponRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);

        if (coupon == null)
        {
            return ApiResponse<bool>.FailureResponse("Coupon not found.");
        }

        var normalizedCode = req.Code.Trim().ToUpperInvariant();

        // If code is changed, verify uniqueness
        if (!coupon.Code.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase))
        {
            var codeInUse = await _couponRepository.ExistsByCodeAsync(normalizedCode, req.Id, cancellationToken);
            if (codeInUse)
            {
                return ApiResponse<bool>.FailureResponse($"A coupon with code '{normalizedCode}' already exists.");
            }
        }

        coupon.Code = normalizedCode;
        coupon.Category = req.Category;
        coupon.Description = req.Description?.Trim();
        coupon.DiscountType = req.DiscountType;
        coupon.DiscountValue = req.DiscountValue;
        coupon.MinOrderAmount = req.MinOrderAmount;
        coupon.MaxDiscountAmount = req.MaxDiscountAmount;
        coupon.ApplicablePlan = req.ApplicablePlan;
        coupon.MaxUses = req.MaxUses;
        coupon.ExpiryDate = req.ExpiryDate;
        coupon.IsActive = req.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;

        _couponRepository.Update(coupon);
        await _couponRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, $"Coupon '{coupon.Code}' updated successfully.");
    }
}
