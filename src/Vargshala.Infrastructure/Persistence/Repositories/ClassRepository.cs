using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class ClassRepository : IClassRepository
{
    private readonly IVargshalaDbContext _db;

    public ClassRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<Class, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return c => (c.Name != null && EF.Functions.Like(c.Name.ToLower(), lowerTerm))
                 || (c.Code != null && EF.Functions.Like(c.Code.ToLower(), lowerTerm))
                 || (c.Description != null && EF.Functions.Like(c.Description.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<Class, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = c => c.Name,
        ["classname"] = c => c.Name,
        ["code"] = c => c.Code,
        ["classcode"] = c => c.Code,
        ["branch"] = c => c.Branch!.Name,
        ["branchname"] = c => c.Branch!.Name,
        ["description"] = c => c.Description!,
        ["isactive"] = c => c.IsActive,
        ["status"] = c => c.IsActive,
        ["createdat"] = c => c.CreatedAt,
        ["updatedat"] = c => c.UpdatedAt!
    };
    #endregion

    public async Task<Class?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Classes
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Batches)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<Class?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Classes
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAndBranchAsync(string code, Guid branchId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Classes
            .Where(c => c.BranchId == branchId && c.Code.ToLower() == code.ToLower() && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasBatchesAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        return await _db.Batches
            .AnyAsync(b => b.ClassId == classId && !b.IsDeleted, cancellationToken);
    }

    public async Task<(List<Class> Items, int TotalRecords)> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? branchId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Classes
            .AsNoTracking()
            .Include(c => c.Branch)
            .Include(c => c.Batches)
            .Where(c => c.Branch!.OrganizationId == organizationId && !c.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(c => c.BranchId == branchId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: c => c.Name,
            defaultAscending: true,
            cancellationToken: cancellationToken);
    }

    public async Task<List<Class>> GetAllActiveAsync(Guid organizationId, Guid? branchId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Classes
            .AsNoTracking()
            .Where(c => c.Branch!.OrganizationId == organizationId && c.IsActive && !c.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(c => c.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Class entity, CancellationToken cancellationToken = default)
    {
        await _db.Classes.AddAsync(entity, cancellationToken);
    }

    public void Update(Class entity)
    {
        _db.Classes.Update(entity);
    }

    public void Delete(Class entity)
    {
        entity.IsDeleted = true;
        _db.Classes.Update(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
