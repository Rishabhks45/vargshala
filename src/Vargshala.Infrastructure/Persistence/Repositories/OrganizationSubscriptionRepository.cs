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
        var plan = await _context.SubscriptionPlans.FirstAsync(p => p.Id == planId, cancellationToken);
        var currentSub = await GetCurrentActiveSubscriptionAsync(organizationId, cancellationToken);

        DateTime startDate;
        DateTime endDate;

        if (currentSub != null && currentSub.EndDate >= DateTime.UtcNow &&
            (currentSub.Status == SubscriptionStatus.Active || currentSub.Status == SubscriptionStatus.Trial))
        {
            startDate = currentSub.StartDate;
            endDate = CalculateEndDate(currentSub.EndDate, plan.BillingCycle);
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
            Status = "Completed",
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
