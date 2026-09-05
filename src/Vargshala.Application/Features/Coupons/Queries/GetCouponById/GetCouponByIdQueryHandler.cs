using MediatR;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Queries.GetCouponById;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, ApiResponse<CouponDto>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByIdQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<CouponDto>> Handle(
        GetCouponByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _couponRepository.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
        {
            return ApiResponse<CouponDto>.FailureResponse("Coupon not found.");
        }

        var dto = new CouponDto
        {
            Id = entity.Id,
            OrganizationId = entity.OrganizationId,
            Code = entity.Code,
            Category = entity.Category,
            Description = entity.Description,
            DiscountType = entity.DiscountType,
            DiscountValue = entity.DiscountValue,
            MinOrderAmount = entity.MinOrderAmount,
            MaxDiscountAmount = entity.MaxDiscountAmount,
            ApplicablePlan = entity.ApplicablePlan,
            UsedCount = entity.UsedCount,
            MaxUses = entity.MaxUses,
            ExpiryDate = entity.ExpiryDate,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

        return ApiResponse<CouponDto>.SuccessResponse(dto);
    }
}
