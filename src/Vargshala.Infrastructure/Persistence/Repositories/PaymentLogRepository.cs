using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Features.Payments.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Payments;
using Vargshala.Contracts.Subscriptions;
using Vargshala.Domain.Entities;
using Vargshala.Infrastructure.Persistence;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class PaymentLogRepository : IPaymentLogRepository
{
    private readonly VargshalaDbContext _context;

    public PaymentLogRepository(VargshalaDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<PaymentLogDto>> GetPaymentLogsPagedAsync(
        PagedRequest request,
        string? method,
        string? status,
        PaymentType? paymentType,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Include(p => p.Organization)
            .Include(p => p.OrganizationSubscription)
                .ThenInclude(s => s!.Plan)
            .AsNoTracking();

        // 1. PaymentType Filter (defaults to Subscription if specified, or all)
        if (paymentType.HasValue)
        {
            query = query.Where(p => p.PaymentType == paymentType.Value);
        }

        // 2. Search Filter (Receipt, Reference/PaymentId, Institute Name, Email)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                (p.ReceiptNumber != null && EF.Functions.ILike(p.ReceiptNumber, $"%{search}%")) ||
                (p.TransactionReference != null && EF.Functions.ILike(p.TransactionReference, $"%{search}%")) ||
                (p.Organization != null && EF.Functions.ILike(p.Organization.Name, $"%{search}%")) ||
                (p.Organization != null && p.Organization.Email != null && EF.Functions.ILike(p.Organization.Email, $"%{search}%")));
        }

        // 3. Method Filter
        if (!string.IsNullOrWhiteSpace(method))
        {
            var methodLower = method.Trim().ToLower();
            query = query.Where(p => EF.Functions.ILike(p.PaymentMethod, $"%{methodLower}%"));
        }

        // 4. Status Filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusLower = status.Trim().ToLower();
            if (statusLower == "captured" || statusLower == "success")
            {
                query = query.Where(p => p.Status.ToLower() == "captured" || p.Status.ToLower() == "completed" || p.Status.ToLower() == "success");
            }
            else
            {
                query = query.Where(p => p.Status.ToLower() == statusLower);
            }
        }

        // 5. Total Count
        var totalCount = await query.CountAsync(cancellationToken);

        // 6. Sorting
        var isAsc = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "amount" => isAsc ? query.OrderBy(p => p.Amount) : query.OrderByDescending(p => p.Amount),
            "method" => isAsc ? query.OrderBy(p => p.PaymentMethod) : query.OrderByDescending(p => p.PaymentMethod),
            "status" => isAsc ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
            "institute" => isAsc ? query.OrderBy(p => p.Organization.Name) : query.OrderByDescending(p => p.Organization.Name),
            "receiptnumber" or "paymentid" => isAsc ? query.OrderBy(p => p.ReceiptNumber) : query.OrderByDescending(p => p.ReceiptNumber),
            _ => isAsc ? query.OrderBy(p => p.PaymentDate) : query.OrderByDescending(p => p.PaymentDate)
        };

        // 7. Paging
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Max(1, Math.Min(100, request.PageSize));

        var pagedEntities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = pagedEntities.Select(p =>
        {
            var receiptNum = p.ReceiptNumber ?? (p.Id.ToString().Length >= 8 ? p.Id.ToString().Substring(0, 8).ToUpperInvariant() : p.Id.ToString());
            var orderId = p.ReceiptNumber != null && p.ReceiptNumber.StartsWith("order_") ? p.ReceiptNumber : null;

            return new PaymentLogDto
            {
                PaymentId = p.Id,
                ReceiptNumber = receiptNum,
                OrderId = orderId,
                TransactionReference = p.TransactionReference,
                OrganizationId = p.OrganizationId,
                InstituteName = p.Organization != null ? p.Organization.Name : "Institute",
                PayerEmail = p.Organization != null ? p.Organization.Email : null,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                PaymentType = p.PaymentType.ToString(),
                PlanName = p.OrganizationSubscription != null && p.OrganizationSubscription.Plan != null 
                    ? p.OrganizationSubscription.Plan.Name 
                    : null,
                Remarks = p.Remarks
            };
        }).ToList();

        return PagedResponse<PaymentLogDto>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaymentLogsStatsDto> GetPaymentLogsStatsAsync(CancellationToken cancellationToken = default)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var query = _context.Payments.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return new PaymentLogsStatsDto();
        }

        var totalVolume30d = await query
            .Where(p => p.PaymentDate >= thirtyDaysAgo && 
                       (p.Status.ToLower() == "completed" || p.Status.ToLower() == "captured" || p.Status.ToLower() == "success"))
            .SumAsync(p => p.Amount, cancellationToken);

        var successfulCount = await query
            .CountAsync(p => p.Status.ToLower() == "completed" || p.Status.ToLower() == "captured" || p.Status.ToLower() == "success", cancellationToken);

        var failedCount = await query
            .CountAsync(p => p.Status.ToLower() == "failed", cancellationToken);

        double successRate = totalCount > 0 
            ? Math.Round((double)successfulCount / totalCount * 100, 1) 
            : 100.0;

        return new PaymentLogsStatsDto
        {
            TotalVolume30Days = totalVolume30d,
            SuccessRate = successRate,
            FailedCount = failedCount,
            TotalTransactionsCount = totalCount,
            SettlementCycle = "T + 1 Days"
        };
    }

    public async Task<SubscriptionPaymentReceiptDto?> GetPaymentReceiptByIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Organization)
            .Include(p => p.OrganizationSubscription)
                .ThenInclude(s => s!.Plan)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

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

        var receiptNum = payment.ReceiptNumber ?? (payment.Id.ToString().Length >= 8 ? payment.Id.ToString().Substring(0, 8).ToUpperInvariant() : payment.Id.ToString());

        return new SubscriptionPaymentReceiptDto
        {
            PaymentId = payment.Id,
            ReceiptNumber = receiptNum,
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
            PlanName = plan?.Name ?? (payment.PaymentType == PaymentType.StudentFee ? "Student Fee" : "Subscription Plan"),
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
}
