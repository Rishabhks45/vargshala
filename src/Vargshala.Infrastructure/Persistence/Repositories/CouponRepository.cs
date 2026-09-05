using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.Coupons.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class CouponRepository : ICouponRepository
{
    #region Fields & Constructor
    private readonly IVargshalaDbContext _db;

    public CouponRepository(IVargshalaDbContext db)
    {
        _db = db;
    }
    #endregion

    #region Search & Sort Mappings
    // ---- Searchable fields for Coupon ----
    private static Func<string, Expression<Func<Coupon, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return c => EF.Functions.Like(c.Code.ToLower(), lowerTerm)
                 || (c.Description != null && EF.Functions.Like(c.Description.ToLower(), lowerTerm));
    };

    // ---- Sortable fields whitelist for Coupon ----
    private static readonly Dictionary<string, Expression<Func<Coupon, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["code"] = c => c.Code,
        ["discount"] = c => c.DiscountValue,
        ["plan"] = c => c.ApplicablePlan,
        ["usage"] = c => c.UsedCount,
        ["expiry"] = c => c.ExpiryDate,
        ["status"] = c => c.IsActive,
        ["isactive"] = c => c.IsActive,
        ["createdat"] = c => c.CreatedAt,
    };
    #endregion

    #region Query Methods
    public async Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<Coupon?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Coupons
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _db.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == normalizedCode && !c.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var query = _db.Coupons.Where(c => c.Code == normalizedCode && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(List<Coupon> Items, int TotalRecords)> GetCouponsPagedAsync(
        PagedRequest request,
        CampaignCategory? category = null,
        DiscountType? discountType = null,
        ApplicablePlan? plan = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Coupons
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (category.HasValue)
        {
            query = query.Where(c => c.Category == category.Value);
        }

        if (discountType.HasValue)
        {
            query = query.Where(c => c.DiscountType == discountType.Value);
        }

        if (plan.HasValue)
        {
            query = query.Where(c => c.ApplicablePlan == plan.Value || c.ApplicablePlan == ApplicablePlan.AllPlans);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: c => c.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }
    #endregion

    #region Command Methods
    public async Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default)
    {
        await _db.Coupons.AddAsync(coupon, cancellationToken);
    }

    public void Update(Coupon coupon)
    {
        _db.Coupons.Update(coupon);
    }

    public void Delete(Coupon coupon)
    {
        coupon.IsDeleted = true;
        coupon.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
