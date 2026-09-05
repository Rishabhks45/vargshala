using MediatR;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, ApiResponse<Guid>>
{
    private readonly ICouponRepository _couponRepository;

    public CreateCouponCommandHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<Guid>> Handle(
        CreateCouponCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var normalizedCode = req.Code.Trim().ToUpperInvariant();

        // Check for duplicate code
        var exists = await _couponRepository.ExistsByCodeAsync(normalizedCode, null, cancellationToken);
        if (exists)
        {
            return ApiResponse<Guid>.FailureResponse($"A coupon with code '{normalizedCode}' already exists.");
        }

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            OrganizationId = req.OrganizationId,
            Code = normalizedCode,
            Category = req.Category,
            Description = req.Description?.Trim(),
            DiscountType = req.DiscountType,
            DiscountValue = req.DiscountValue,
            MinOrderAmount = req.MinOrderAmount,
            MaxDiscountAmount = req.MaxDiscountAmount,
            ApplicablePlan = req.ApplicablePlan,
            UsedCount = 0,
            MaxUses = req.MaxUses,
            ExpiryDate = req.ExpiryDate,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _couponRepository.AddAsync(coupon, cancellationToken);
        await _couponRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(coupon.Id, $"Coupon '{normalizedCode}' created successfully.");
    }
}
