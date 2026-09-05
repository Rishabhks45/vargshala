using MediatR;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Coupons;

namespace Vargshala.Application.Features.Coupons.Queries.GetCoupons;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, ApiResponse<PagedResponse<CouponDto>>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponsQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<PagedResponse<CouponDto>>> Handle(
        GetCouponsQuery request,
        CancellationToken cancellationToken)
    {
        var pagedRequest = request.Request ?? new PagedRequest();

        var (items, totalRecords) = await _couponRepository.GetCouponsPagedAsync(
            pagedRequest,
            request.Category,
            request.DiscountType,
            request.Plan,
            request.IsActive,
            cancellationToken);

        var dtos = items.Select(c => new CouponDto
        {
            Id = c.Id,
            OrganizationId = c.OrganizationId,
            Code = c.Code,
            Category = c.Category,
            Description = c.Description,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            MinOrderAmount = c.MinOrderAmount,
            MaxDiscountAmount = c.MaxDiscountAmount,
            ApplicablePlan = c.ApplicablePlan,
            UsedCount = c.UsedCount,
            MaxUses = c.MaxUses,
            ExpiryDate = c.ExpiryDate,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        var pagedResponse = PagedResponse<CouponDto>.Create(dtos, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<CouponDto>>.SuccessResponse(pagedResponse);
    }
}
