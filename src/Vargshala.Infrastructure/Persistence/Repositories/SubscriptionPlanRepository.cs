using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.SubscriptionPlans.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;
using Vargshala.SharedKernel.Enums;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly IVargshalaDbContext _db;

    public SubscriptionPlanRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    private static readonly Dictionary<string, Expression<Func<SubscriptionPlan, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = p => p.Name,
        ["price"] = p => p.Price,
        ["billingcycle"] = p => p.BillingCycle,
        ["maxstudents"] = p => p.MaxStudents ?? int.MaxValue,
        ["maxteachers"] = p => p.MaxTeachers ?? int.MaxValue,
        ["maxbranches"] = p => p.MaxBranches ?? int.MaxValue,
        ["isactive"] = p => p.IsActive,
        ["status"] = p => p.IsActive,
        ["createdat"] = p => p.CreatedAt,
    };

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower() && !p.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.SubscriptionPlans.Where(p => p.Name.ToLower() == name.ToLower() && !p.IsDeleted);
        if (excludeId.HasValue && excludeId.Value != Guid.Empty)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<SubscriptionPlan>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Price)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<SubscriptionPlan> Items, int TotalRecords)> GetPagedAsync(
        PagedRequest request,
        bool? isActive = null,
        BillingCycle? billingCycle = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.Subscriptions)
            .Where(p => !p.IsDeleted);

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim().ToLower()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, term)
                                  || (p.Description != null && EF.Functions.ILike(p.Description, term)));
        }

        // Filters
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (billingCycle.HasValue)
        {
            query = query.Where(p => p.BillingCycle == billingCycle.Value);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        // Sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy) &&
            SortMappings.TryGetValue(request.SortBy, out var sortExpression))
        {
            query = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(sortExpression)
                : query.OrderBy(sortExpression);
        }
        else
        {
            query = query.OrderBy(p => p.Price);
        }

        // Paging
        var skip = (request.PageNumber - 1) * request.PageSize;
        var items = await query
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }

    public async Task AddAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
    {
        await _db.SubscriptionPlans.AddAsync(plan, cancellationToken);
    }

    public void Update(SubscriptionPlan plan)
    {
        _db.SubscriptionPlans.Update(plan);
    }

    public void Delete(SubscriptionPlan plan)
    {
        _db.SubscriptionPlans.Update(plan);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
