using FluentValidation;
using MediatR;
using RazorpayUtility.Interfaces;
using RazorpayUtility.Models;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Subscriptions;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Application.Features.OrganizationSubscriptions.Commands.ConfirmSubscriptionPayment;

public record ConfirmSubscriptionPaymentCommand(ConfirmSubscriptionPaymentRequest Request)
    : IRequest<ApiResponse<OrganizationSubscriptionDto>>;

public class ConfirmSubscriptionPaymentCommandValidator : AbstractValidator<ConfirmSubscriptionPaymentCommand>
{
    public ConfirmSubscriptionPaymentCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request).NotNull().WithMessage("Confirmation request is required.");
        RuleFor(x => x.Request.PlanId).NotEmpty().WithMessage("Subscription plan must be selected.");
    }
}

public class ConfirmSubscriptionPaymentCommandHandler
    : IRequestHandler<ConfirmSubscriptionPaymentCommand, ApiResponse<OrganizationSubscriptionDto>>
{
    private readonly IOrganizationSubscriptionRepository _repository;
    private readonly IRazorpayPaymentService _razorpayPaymentService;
    private readonly ICurrentUser _currentUser;

    public ConfirmSubscriptionPaymentCommandHandler(
        IOrganizationSubscriptionRepository repository,
        IRazorpayPaymentService razorpayPaymentService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _razorpayPaymentService = razorpayPaymentService;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<OrganizationSubscriptionDto>> Handle(
        ConfirmSubscriptionPaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.OrganizationId.HasValue)
        {
            return ApiResponse<OrganizationSubscriptionDto>.FailureResponse("User does not belong to an organization.");
        }

        var req = command.Request;
        var plan = await _repository.GetPlanByIdAsync(req.PlanId, cancellationToken);
        if (plan == null || !plan.IsActive)
        {
            return ApiResponse<OrganizationSubscriptionDto>.FailureResponse("Selected subscription plan was not found or is inactive.");
        }

        decimal originalAmount = plan.Price;
        decimal discountAmount = 0m;
        Guid? couponIdToIncrement = null;

        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var code = req.CouponCode.Trim();
            var coupon = await _repository.GetCouponByCodeAsync(code, cancellationToken);
            if (coupon != null && coupon.IsActive && coupon.ExpiryDate >= DateTime.UtcNow && coupon.UsedCount < coupon.MaxUses)
            {
                couponIdToIncrement = coupon.Id;
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
            }
        }

        decimal finalAmount = Math.Max(0, originalAmount - discountAmount);

        // For paid subscriptions, verify the cryptographic signature from Razorpay
        if (finalAmount > 0)
        {
            if (string.IsNullOrWhiteSpace(req.RazorpayOrderId) ||
                string.IsNullOrWhiteSpace(req.RazorpayPaymentId) ||
                string.IsNullOrWhiteSpace(req.RazorpaySignature))
            {
                return ApiResponse<OrganizationSubscriptionDto>.FailureResponse("Razorpay payment details (Order ID, Payment ID, Signature) are required for paid plans.");
            }

            var verifyResult = await _razorpayPaymentService.VerifyPaymentSignatureAsync(new RazorpayPaymentVerifyRequest
            {
                RazorpayOrderId = req.RazorpayOrderId,
                RazorpayPaymentId = req.RazorpayPaymentId,
                RazorpaySignature = req.RazorpaySignature
            }, cancellationToken);

            if (!verifyResult.IsSuccess || !verifyResult.IsValidSignature)
            {
                return ApiResponse<OrganizationSubscriptionDto>.FailureResponse(verifyResult.ErrorMessage ?? "Payment verification failed. Invalid cryptographic signature.");
            }
        }

        // Increment coupon usage if validly applied
        if (couponIdToIncrement.HasValue)
        {
            await _repository.IncrementCouponUsedCountAsync(couponIdToIncrement.Value, cancellationToken);
        }

        string paymentMethod = finalAmount > 0 ? "Razorpay" : "Free";
        string transactionRef = !string.IsNullOrWhiteSpace(req.RazorpayPaymentId) ? req.RazorpayPaymentId : "FREE-PLAN";
        string receiptNumber = !string.IsNullOrWhiteSpace(req.RazorpayOrderId) ? req.RazorpayOrderId : $"SUB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..Math.Min(30, 25)];
        string remarks = $"Subscription: {plan.Name}{(string.IsNullOrWhiteSpace(req.CouponCode) ? string.Empty : $", Coupon: {req.CouponCode}")}";

        var subscription = await _repository.CreateOrRenewSubscriptionAsync(
            _currentUser.OrganizationId.Value,
            plan.Id,
            finalAmount,
            paymentMethod,
            transactionRef,
            receiptNumber,
            remarks,
            cancellationToken);

        var dto = new OrganizationSubscriptionDto
        {
            Id = subscription.Id,
            OrganizationId = subscription.OrganizationId,
            OrganizationName = subscription.Organization?.Name ?? string.Empty,
            PlanId = subscription.PlanId,
            PlanName = plan.Name,
            PlanPrice = plan.Price,
            BillingCycle = plan.BillingCycle,
            MaxStudents = plan.MaxStudents,
            MaxTeachers = plan.MaxTeachers,
            MaxBranches = plan.MaxBranches,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            Status = subscription.Status,
            AutoRenew = subscription.AutoRenew,
            CreatedAt = subscription.CreatedAt
        };

        return ApiResponse<OrganizationSubscriptionDto>.SuccessResponse(dto, "Subscription activated successfully!");
    }
}
