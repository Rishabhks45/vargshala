using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class FeeStructureRepository : IFeeStructureRepository
{
    private readonly IVargshalaDbContext _db;

    public FeeStructureRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<FeeStructure, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return fs => (fs.Name != null && EF.Functions.Like(fs.Name.ToLower(), lowerTerm))
                  || (fs.Description != null && EF.Functions.Like(fs.Description.ToLower(), lowerTerm))
                  || (fs.AcademicSession != null && EF.Functions.Like(fs.AcademicSession.ToLower(), lowerTerm))
                  || (fs.Branch != null && fs.Branch.Name != null && EF.Functions.Like(fs.Branch.Name.ToLower(), lowerTerm))
                  || (fs.Class != null && fs.Class.Name != null && EF.Functions.Like(fs.Class.Name.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<FeeStructure, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = fs => fs.Name,
        ["feestructurename"] = fs => fs.Name,
        ["amount"] = fs => fs.TotalAmount,
        ["totalamount"] = fs => fs.TotalAmount,
        ["session"] = fs => fs.AcademicSession,
        ["academicsession"] = fs => fs.AcademicSession,
        ["branch"] = fs => fs.Branch!.Name,
        ["branchname"] = fs => fs.Branch!.Name,
        ["class"] = fs => fs.Class!.Name,
        ["classname"] = fs => fs.Class!.Name,
        ["isactive"] = fs => fs.IsActive,
        ["status"] = fs => fs.IsActive,
        ["createdat"] = fs => fs.CreatedAt,
        ["updatedat"] = fs => fs.UpdatedAt!
    };
    #endregion

    public async Task<FeeStructure?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.FeeStructures
            .FirstOrDefaultAsync(fs => fs.Id == id && !fs.IsDeleted, cancellationToken);
    }

    public async Task<FeeStructure?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.FeeStructures
            .AsNoTracking()
            .Include(fs => fs.Branch)
            .Include(fs => fs.Class)
            .FirstOrDefaultAsync(fs => fs.Id == id && !fs.IsDeleted, cancellationToken);
    }

    public async Task<(List<FeeStructure> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        string? session = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FeeStructures
            .AsNoTracking()
            .Include(fs => fs.Branch)
            .Include(fs => fs.Class)
            .Where(fs => fs.OrganizationId == organizationId && !fs.IsDeleted);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query = query.Where(fs => fs.BranchId == branchId.Value);
        }

        if (classId.HasValue && classId.Value != Guid.Empty)
        {
            query = query.Where(fs => fs.ClassId == classId.Value);
        }

        if (!string.IsNullOrWhiteSpace(session))
        {
            query = query.Where(fs => fs.AcademicSession == session.Trim());
        }

        if (isActive.HasValue)
        {
            query = query.Where(fs => fs.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: fs => fs.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }

    public async Task<List<FeeStructure>> GetAllActiveAsync(
        Guid organizationId,
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FeeStructures
            .AsNoTracking()
            .Where(fs => fs.OrganizationId == organizationId && fs.IsActive && !fs.IsDeleted);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            query = query.Where(fs => fs.BranchId == branchId.Value);
        }

        if (classId.HasValue && classId.Value != Guid.Empty)
        {
            query = query.Where(fs => fs.ClassId == classId.Value);
        }

        return await query
            .OrderBy(fs => fs.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAndBranchAsync(
        string name,
        Guid branchId,
        string session,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FeeStructures
            .Where(fs => fs.BranchId == branchId &&
                         fs.Name.ToLower() == name.ToLower() &&
                         fs.AcademicSession.ToLower() == session.ToLower() &&
                         !fs.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(fs => fs.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(FeeStructure feeStructure, CancellationToken cancellationToken = default)
    {
        await _db.FeeStructures.AddAsync(feeStructure, cancellationToken);
    }

    public void Update(FeeStructure feeStructure)
    {
        _db.FeeStructures.Update(feeStructure);
    }

    public void Delete(FeeStructure feeStructure)
    {
        feeStructure.IsDeleted = true;
        _db.FeeStructures.Update(feeStructure);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
