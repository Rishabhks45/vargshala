using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Features.OrganizationSubscriptions.Infrastructure;
using Vargshala.Contracts.Subscriptions;
using Vargshala.Domain.Entities;
using Vargshala.Infrastructure.Persistence;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class OrganizationSubscriptionRepository : IOrganizationSubscriptionRepository
{
    private readonly VargshalaDbContext _context;

    public OrganizationSubscriptionRepository(VargshalaDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizationSubscription?> GetCurrentActiveSubscriptionAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.OrganizationSubscriptions
            .Include(s => s.Plan)
            .Include(s => s.Organization)
            .Where(s => s.OrganizationId == organizationId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<QuotaUsageDto> GetQuotaUsageAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await GetCurrentActiveSubscriptionAsync(organizationId, cancellationToken);

        var studentsCount = await _context.Students
            .CountAsync(s => s.User.OrganizationId == organizationId && s.IsActive && !s.IsDeleted, cancellationToken);

        var teachersCount = await _context.Teachers
            .CountAsync(t => t.User.OrganizationId == organizationId && t.IsActive && !t.IsDeleted, cancellationToken);

        var branchesCount = await _context.Branches
            .CountAsync(b => b.OrganizationId == organizationId && b.IsActive && !b.IsDeleted, cancellationToken);

        OrganizationSubscriptionDto? subDto = null;
        if (subscription != null)
        {
            subDto = new OrganizationSubscriptionDto
            {
                Id = subscription.Id,
                OrganizationId = subscription.OrganizationId,
                OrganizationName = subscription.Organization?.Name ?? string.Empty,
                PlanId = subscription.PlanId,
                PlanName = subscription.Plan?.Name ?? string.Empty,
                PlanPrice = subscription.Plan?.Price ?? 0m,
                BillingCycle = subscription.Plan?.BillingCycle ?? BillingCycle.Monthly,
                MaxStudents = subscription.Plan?.MaxStudents,
                MaxTeachers = subscription.Plan?.MaxTeachers,
                MaxBranches = subscription.Plan?.MaxBranches,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Status = subscription.Status,
                AutoRenew = subscription.AutoRenew,
                CreatedAt = subscription.CreatedAt
            };
        }

        return new QuotaUsageDto
        {
            CurrentSubscription = subDto,
            CurrentStudentsCount = studentsCount,
            MaxStudents = subscription?.Plan?.MaxStudents,
            CurrentTeachersCount = teachersCount,
            MaxTeachers = subscription?.Plan?.MaxTeachers,
            CurrentBranchesCount = branchesCount,
            MaxBranches = subscription?.Plan?.MaxBranches
        };
    }

    public async Task<List<SubscriptionBillingHistoryDto>> GetBillingHistoryAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var payments = await _context.Payments
            .Include(p => p.OrganizationSubscription)
                .ThenInclude(s => s!.Plan)
            .Where(p => p.OrganizationId == organizationId && p.PaymentType == PaymentType.Subscription)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        return payments.Select(p => new SubscriptionBillingHistoryDto
        {
            PaymentId = p.Id,
            ReceiptNumber = p.ReceiptNumber ?? p.Id.ToString()[..8].ToUpperInvariant(),
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.PaymentMethod,
            TransactionReference = p.TransactionReference,
            Status = p.Status,
            PlanName = p.OrganizationSubscription?.Plan?.Name ?? "Subscription Plan",
            Remarks = p.Remarks
        }).ToList();
    }

    public async Task<SubscriptionPaymentReceiptDto?> GetSubscriptionPaymentReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Organization)
            .Include(p => p.OrganizationSubscription)
                .ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.PaymentType == PaymentType.Subscription, cancellationToken);

        if (payment == null) return null;

        var plan = payment.OrganizationSubscription?.Plan;
        var org = payment.Organization;

        string? couponCode = null;
        if (!string.IsNullOrWhiteSpace(payment.Remarks) && payment.Remarks.Contains("Coupon:", StringComparison.OrdinalIgnoreCase))
        {
            var idx = payment.Remarks.IndexOf("Coupon:", StringComparison.OrdinalIgnoreCase);
            couponCode = payment.Remarks[(idx + 7)..].Trim();
        }

        decimal originalAmount = plan?.Price ?? payment.Amount;
        decimal discountAmount = Math.Max(0, originalAmount - payment.Amount);

        return new SubscriptionPaymentReceiptDto
        {
            PaymentId = payment.Id,
            ReceiptNumber = payment.ReceiptNumber ?? payment.Id.ToString()[..8].ToUpperInvariant(),
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            OriginalAmount = originalAmount,
            DiscountAmount = discountAmount,
            Currency = "INR",
            PaymentMethod = payment.PaymentMethod,
            TransactionReference = payment.TransactionReference,
            RazorpayOrderId = payment.ReceiptNumber?.StartsWith("order_") == true ? payment.ReceiptNumber : null,
            Status = payment.Status,
            Remarks = payment.Remarks,
            CouponCode = couponCode,

            OrganizationId = payment.OrganizationId,
            OrganizationName = org?.Name ?? "Institute",
            OrganizationEmail = org?.Email,
            OrganizationPhone = org?.Mobile,
            OrganizationAddress = org?.Address,

            PlanId = plan?.Id,
            PlanName = plan?.Name ?? "Subscription Plan",
            BillingCycle = plan?.BillingCycle.ToString() ?? "Monthly",
            StudentQuota = plan?.MaxStudents.HasValue == true ? $"{plan.MaxStudents.Value:N0} Students" : "Unlimited",
            TeacherQuota = plan?.MaxTeachers.HasValue == true ? $"{plan.MaxTeachers.Value:N0} Faculty" : "Unlimited",
            BranchQuota = plan?.MaxBranches.HasValue == true ? $"{plan.MaxBranches.Value:N0} Branches" : "Unlimited",
            SubscriptionStartDate = payment.OrganizationSubscription?.StartDate,
            SubscriptionEndDate = payment.OrganizationSubscription?.EndDate,

            IssuerName = "Vargshala EdTech SaaS",
            IssuerWebsite = "https://vargshala.com",
            IssuerSupportEmail = "billing@vargshala.com"
        };
    }

    public async Task<SubscriptionPlan?> GetPlanByIdAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive, cancellationToken);
    }

    public async Task<Coupon?> GetCouponByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower(), cancellationToken);
    }

    public async Task IncrementCouponUsedCountAsync(
        Guid couponId,
        CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);
        if (coupon != null)
        {
            coupon.UsedCount += 1;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<OrganizationSubscription> CreateOrRenewSubscriptionAsync(
        Guid organizationId,
        Guid planId,
        decimal amount,
        string paymentMethod,
        string transactionRef,
        string receiptNumber,
        string? remarks,
        CancellationToken cancellationToken = default)
    {
        // 1. Idempotency Check: Prevent duplicate subscription/payment creation if both Webhook & UI confirm arrive
        if (!string.IsNullOrWhiteSpace(transactionRef) && transactionRef != "FREE-PLAN")
        {
            var existingPayment = await _context.Payments
                .Include(p => p.OrganizationSubscription)
                    .ThenInclude(s => s!.Plan)
                .Include(p => p.OrganizationSubscription)
                    .ThenInclude(s => s!.Organization)
                .FirstOrDefaultAsync(p => p.TransactionReference == transactionRef || (receiptNumber != null && p.ReceiptNumber == receiptNumber), cancellationToken);

            if (existingPayment?.OrganizationSubscription != null)
            {
                if (existingPayment.Status != PaymentStatuses.Completed)
                {
                    existingPayment.Status = PaymentStatuses.Completed;
                    if (!string.IsNullOrWhiteSpace(remarks))
                    {
                        existingPayment.Remarks = remarks;
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return existingPayment.OrganizationSubscription;
            }
        }

        var plan = await _context.SubscriptionPlans.FirstAsync(p => p.Id == planId, cancellationToken);
        var currentSub = await GetCurrentActiveSubscriptionAsync(organizationId, cancellationToken);

        DateTime startDate;
        DateTime endDate;

        if (currentSub != null && currentSub.EndDate >= DateTime.UtcNow &&
            (currentSub.Status == SubscriptionStatus.Active || currentSub.Status == SubscriptionStatus.Trial))
        {
            if (currentSub.PlanId == plan.Id)
            {
                // Renewal of the same plan
                startDate = currentSub.StartDate;
                endDate = CalculateEndDate(currentSub.EndDate, plan.BillingCycle);
            }
            else
            {
                // Plan change (upgrade / downgrade) -> expire existing subscription
                currentSub.Status = SubscriptionStatus.Expired;
                currentSub.AutoRenew = false;
                currentSub.UpdatedAt = DateTime.UtcNow;
                startDate = DateTime.UtcNow;
                endDate = CalculateEndDate(startDate, plan.BillingCycle);
            }
        }
        else
        {
            startDate = DateTime.UtcNow;
            endDate = CalculateEndDate(startDate, plan.BillingCycle);
        }

        var newSub = new OrganizationSubscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            PlanId = plan.Id,
            StartDate = startDate,
            EndDate = endDate,
            Status = plan.Price == 0 && plan.BillingCycle == BillingCycle.Trial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
            AutoRenew = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.OrganizationSubscriptions.Add(newSub);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            OrganizationSubscriptionId = newSub.Id,
            PaymentType = PaymentType.Subscription,
            Amount = amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = paymentMethod,
            TransactionReference = transactionRef,
            ReceiptNumber = receiptNumber,
            Status = PaymentStatuses.Completed,
            Remarks = remarks,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.OrganizationSubscriptions
            .Include(s => s.Plan)
            .Include(s => s.Organization)
            .FirstAsync(s => s.Id == newSub.Id, cancellationToken);
    }

    public async Task RecordFailedPaymentAsync(
        Guid? organizationId,
        decimal amount,
        string paymentMethod,
        string transactionRef,
        string receiptNumber,
        string? failureReason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionRef)) return;

        var existing = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionReference == transactionRef, cancellationToken);

        if (existing != null)
        {
            existing.Status = PaymentStatuses.Failed;
            existing.Remarks = failureReason ?? "Payment failed on gateway";
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (organizationId.HasValue && organizationId.Value != Guid.Empty)
        {
            var failedPayment = new Payment
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId.Value,
                PaymentType = PaymentType.Subscription,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                TransactionReference = transactionRef,
                ReceiptNumber = receiptNumber,
                Status = PaymentStatuses.Failed,
                Remarks = failureReason ?? "Payment failed on gateway",
                CreatedAt = DateTime.UtcNow,
                IsActive = false
            };

            _context.Payments.Add(failedPayment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RecordRefundAsync(
        string paymentTransactionRef,
        string refundId,
        decimal refundAmount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentTransactionRef)) return;

        var payment = await _context.Payments
            .Include(p => p.OrganizationSubscription)
            .FirstOrDefaultAsync(p => p.TransactionReference == paymentTransactionRef, cancellationToken);

        if (payment != null)
        {
            payment.Status = PaymentStatuses.Refunded;
            payment.Remarks = $"Refund Processed: {refundId} (₹{refundAmount:N2}). {payment.Remarks}".Trim();

            if (payment.OrganizationSubscription != null && refundAmount >= payment.Amount)
            {
                payment.OrganizationSubscription.Status = SubscriptionStatus.Cancelled;
                payment.OrganizationSubscription.IsActive = false;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Payment?> GetPaymentByTransactionRefAsync(
        string transactionRef,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionRef)) return null;

        return await _context.Payments
            .AsNoTracking()
            .Include(p => p.OrganizationSubscription)
                .ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(p => p.TransactionReference == transactionRef, cancellationToken);
    }

    public async Task<bool> CancelSubscriptionAsync(
        Guid organizationId,
        string? cancelReason = null,
        CancellationToken cancellationToken = default)
    {
        var activeSub = await _context.OrganizationSubscriptions
            .Where(s => s.OrganizationId == organizationId &&
                        (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSub == null) return false;

        activeSub.Status = SubscriptionStatus.Cancelled;
        activeSub.AutoRenew = false;
        activeSub.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(cancelReason))
        {
            var latestPayment = await _context.Payments
                .Where(p => p.OrganizationSubscriptionId == activeSub.Id)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestPayment != null)
            {
                latestPayment.Remarks = string.IsNullOrWhiteSpace(latestPayment.Remarks)
                    ? $"Cancelled: {cancelReason}"
                    : $"{latestPayment.Remarks} | Cancelled: {cancelReason}";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OrganizationSubscription> ChangeSubscriptionPlanDirectAsync(
        Guid organizationId,
        Guid targetPlanId,
        string? remarks = null,
        CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FirstAsync(p => p.Id == targetPlanId, cancellationToken);

        // Expire all current active/trial subscriptions for this organization
        var currentSubs = await _context.OrganizationSubscriptions
            .Where(s => s.OrganizationId == organizationId &&
                        (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial))
            .ToListAsync(cancellationToken);

        foreach (var sub in currentSubs)
        {
            sub.Status = SubscriptionStatus.Expired;
            sub.AutoRenew = false;
            sub.UpdatedAt = DateTime.UtcNow;
        }

        DateTime startDate = DateTime.UtcNow;
        DateTime endDate = CalculateEndDate(startDate, plan.BillingCycle);

        var newSub = new OrganizationSubscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            PlanId = plan.Id,
            StartDate = startDate,
            EndDate = endDate,
            Status = plan.Price == 0 && plan.BillingCycle == BillingCycle.Trial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
            AutoRenew = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.OrganizationSubscriptions.Add(newSub);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            OrganizationSubscriptionId = newSub.Id,
            PaymentType = PaymentType.Subscription,
            Amount = plan.Price,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "DirectChange",
            TransactionReference = $"CHANGE-{Guid.NewGuid():N}"[..25],
            ReceiptNumber = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..25],
            Status = PaymentStatuses.Completed,
            Remarks = remarks ?? $"Direct plan change to {plan.Name}",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.OrganizationSubscriptions
            .Include(s => s.Plan)
            .Include(s => s.Organization)
            .FirstAsync(s => s.Id == newSub.Id, cancellationToken);
    }

    private static DateTime CalculateEndDate(DateTime baseDate, BillingCycle cycle)
    {
        return cycle switch
        {
            BillingCycle.Trial => baseDate.AddDays(7),
            BillingCycle.Monthly => baseDate.AddMonths(1),
            BillingCycle.Quarterly => baseDate.AddMonths(3),
            BillingCycle.Yearly => baseDate.AddYears(1),
            _ => baseDate.AddMonths(1)
        };
    }
}
