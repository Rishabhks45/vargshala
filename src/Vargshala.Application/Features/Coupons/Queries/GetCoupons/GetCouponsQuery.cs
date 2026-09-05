using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Queries.GetCoupons;

public record GetCouponsQuery(
    PagedRequest? Request = null,
    CampaignCategory? Category = null,
    DiscountType? DiscountType = null,
    ApplicablePlan? Plan = null,
    bool? IsActive = null
) : IRequest<ApiResponse<PagedResponse<CouponDto>>>;
