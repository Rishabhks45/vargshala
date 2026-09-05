using MediatR;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.Coupons.Commands.DeleteCoupon;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, ApiResponse<bool>>
{
    private readonly ICouponRepository _couponRepository;

    public DeleteCouponCommandHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteCouponCommand command,
        CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetByIdForUpdateAsync(command.Id, cancellationToken);

        if (coupon == null)
        {
            return ApiResponse<bool>.FailureResponse("Coupon not found.");
        }

        // Soft delete
        _couponRepository.Delete(coupon);
        await _couponRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, $"Coupon '{coupon.Code}' removed successfully.");
    }
}
