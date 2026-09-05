using MediatR;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Queries.GetCouponByCode;

public class GetCouponByCodeQueryHandler : IRequestHandler<GetCouponByCodeQuery, ApiResponse<CouponDto>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByCodeQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<CouponDto>> Handle(
        GetCouponByCodeQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return ApiResponse<CouponDto>.FailureResponse("Coupon code cannot be empty.");
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var entity = await _couponRepository.GetByCodeAsync(normalizedCode, cancellationToken);

        if (entity == null)
        {
            return ApiResponse<CouponDto>.FailureResponse($"Coupon code '{normalizedCode}' is invalid.");
        }

        if (!entity.IsActive)
        {
            return ApiResponse<CouponDto>.FailureResponse($"Coupon code '{normalizedCode}' is currently inactive.");
        }

        if (entity.ExpiryDate < DateTime.UtcNow.Date)
        {
            return ApiResponse<CouponDto>.FailureResponse($"Coupon code '{normalizedCode}' has expired.");
        }

        if (entity.UsedCount >= entity.MaxUses)
        {
            return ApiResponse<CouponDto>.FailureResponse($"Coupon code '{normalizedCode}' has reached its maximum usage quota.");
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
