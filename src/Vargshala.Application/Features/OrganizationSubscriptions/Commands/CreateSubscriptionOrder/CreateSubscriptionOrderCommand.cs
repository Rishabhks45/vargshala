using FluentValidation;
using MediatR;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Commands.CreateSubscriptionOrder;

public record CreateSubscriptionOrderCommand(SubscriptionCheckoutRequest Request)
    : IRequest<ApiResponse<SubscriptionCheckoutResponse>>;

public class CreateSubscriptionOrderCommandValidator : AbstractValidator<CreateSubscriptionOrderCommand>
{
    public CreateSubscriptionOrderCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request).NotNull().WithMessage("Checkout request is required.");
        RuleFor(x => x.Request.PlanId).NotEmpty().WithMessage("Subscription plan must be selected.");
    }
}

public class CreateSubscriptionOrderCommandHandler
    : IRequestHandler<CreateSubscriptionOrderCommand, ApiResponse<SubscriptionCheckoutResponse>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly IRazorpayOrderService _razorpayOrderService;
    private readonly ICurrentUser _currentUser;

    public CreateSubscriptionOrderCommandHandler(
        IOrganizationSubscriptionRepository repository,
        IRazorpayOrderService razorpayOrderService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _razorpayOrderService = razorpayOrderService;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<SubscriptionCheckoutResponse>> Handle(
        CreateSubscriptionOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse("User does not belong to an organization.");
        }

        var req = command.Request;
        var plan = await _repository.GetPlanByIdAsync(req.PlanId, cancellationToken);
        if (plan == null || !plan.IsActive)
        {
            return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse("Selected subscription plan was not found or is inactive.");
        }

        decimal originalAmount = plan.Price;
        decimal discountAmount = 0m;
        string? couponDescription = null;

        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var code = req.CouponCode.Trim();
            var coupon = await _repository.GetCouponByCodeAsync(code, cancellationToken);
            if (coupon == null || !coupon.IsActive)
            {
                return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse("Invalid coupon code.");
            }

            if (coupon.ExpiryDate < DateTime.UtcNow)
            {
                return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse("Coupon has expired.");
            }

            if (coupon.UsedCount >= coupon.MaxUses)
            {
                return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse("Coupon usage limit has been reached.");
            }

            if (coupon.MinOrderAmount.HasValue && originalAmount < coupon.MinOrderAmount.Value)
            {
                return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse($"Coupon requires a minimum order amount of ₹{coupon.MinOrderAmount.Value:N2}.");
            }

            if (coupon.PlanId.HasValue && coupon.PlanId.Value != plan.Id)
            {
                return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse("This coupon is not valid for the selected plan.");
            }

            if (coupon.DiscountType == DiscountType.Percentage)
            {
                discountAmount = Math.Round(originalAmount * (coupon.DiscountValue / 100m), 2);
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }
            }
            else
            {
                discountAmount = Math.Min(originalAmount, coupon.DiscountValue);
            }

            couponDescription = coupon.Description ?? $"₹{discountAmount:N2} discount applied";
        }

        decimal finalAmount = Math.Max(0, originalAmount - discountAmount);

        if (finalAmount <= 0)
        {
            return ApiResponse<SubscriptionCheckoutResponse>.SuccessResponse(new SubscriptionCheckoutResponse
            {
                IsFreePlan = true,
                OriginalAmount = originalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = 0,
                Currency = "INR",
                PlanName = plan.Name,
                CouponCode = req.CouponCode,
                CouponDescription = couponDescription
            });
        }

        var receipt = $"SUB-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(40, 36)];
        var notes = new Dictionary<string, string>
        {
            ["OrganizationId"] = _currentUser.OrganizationId.Value.ToString(),
            ["PlanId"] = plan.Id.ToString(),
            ["PlanName"] = plan.Name,
            ["CouponCode"] = req.CouponCode ?? string.Empty
        };

        var orderResult = await _razorpayOrderService.CreateOrderAsync(new RazorpayCreateOrderRequest
        {
            Amount = finalAmount,
            Receipt = receipt,
            Notes = notes
        }, cancellationToken);

        if (!orderResult.IsSuccess)
        {
            return ApiResponse<SubscriptionCheckoutResponse>.FailureResponse(orderResult.ErrorMessage ?? "Failed to initialize payment gateway order.");
        }

        return ApiResponse<SubscriptionCheckoutResponse>.SuccessResponse(new SubscriptionCheckoutResponse
        {
            RazorpayOrderId = orderResult.OrderId,
            RazorpayKeyId = orderResult.KeyId,
            OriginalAmount = originalAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            Currency = orderResult.Currency,
            PlanName = plan.Name,
            CompanyName = orderResult.CompanyName,
            ThemeColor = orderResult.ThemeColor,
            CouponCode = req.CouponCode,
            CouponDescription = couponDescription,
            IsFreePlan = false
        });
    }
}
