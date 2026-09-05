using MediatR;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Coupons.Commands.ToggleCouponStatus;

public class ToggleCouponStatusCommandHandler : IRequestHandler<ToggleCouponStatusCommand, ApiResponse<bool>>
{
    private readonly ICouponRepository _couponRepository;

    public ToggleCouponStatusCommandHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<bool>> Handle(
        ToggleCouponStatusCommand command,
        CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);

        if (coupon == null)
        {
            return ApiResponse<bool>.FailureResponse("Coupon not found.");
        }

        coupon.IsActive = !coupon.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;

        _couponRepository.Update(coupon);
        await _couponRepository.SaveChangesAsync(cancellationToken);

        var status = coupon.IsActive ? "activated" : "deactivated";
        return ApiResponse<bool>.SuccessResponse(coupon.IsActive, $"Coupon '{coupon.Code}' has been {status}.");
    }
}
