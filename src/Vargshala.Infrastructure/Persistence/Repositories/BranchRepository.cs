using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public BranchRepository(IVargshalaDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<Branch, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return b => (b.Name != null && EF.Functions.Like(b.Name.ToLower(), lowerTerm))
          || (b.Code != null && EF.Functions.Like(b.Code.ToLower(), lowerTerm))
          || (b.City != null && EF.Functions.Like(b.City.ToLower(), lowerTerm))
          || (b.State != null && EF.Functions.Like(b.State.ToLower(), lowerTerm))
          || (b.Address != null && EF.Functions.Like(b.Address.ToLower(), lowerTerm))
          || (b.Email != null && EF.Functions.Like(b.Email.ToLower(), lowerTerm))
          || (b.Mobile != null && EF.Functions.Like(b.Mobile, $"%{term}%"));
    };

    private static readonly Dictionary<string, Expression<Func<Branch, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = b => b.Name,
        ["code"] = b => b.Code,
        ["city"] = b => b.City!,
        ["state"] = b => b.State!,
        ["email"] = b => b.Email!,
        ["mobile"] = b => b.Mobile!,
        ["phone"] = b => b.Mobile!,
        ["isactive"] = b => b.IsActive,
        ["status"] = b => b.IsActive,
        ["ismainbranch"] = b => b.IsMainBranch,
        ["createdat"] = b => b.CreatedAt,
    };
    #endregion

    #region Query Methods
    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Branches
            .AsNoTracking()
            .Include(b => b.UserBranchAccesses.Where(uba => uba.IsActive))
                .ThenInclude(uba => uba.User)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<Branch?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Branches
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<Branch?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var orgId = _currentUser.OrganizationId;
        var query = _db.Branches.AsNoTracking().Where(b => b.Code == code && !b.IsDeleted);

        if (orgId.HasValue && orgId.Value != Guid.Empty)
        {
            query = query.Where(b => b.OrganizationId == orgId.Value);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var orgId = _currentUser.OrganizationId;
        var query = _db.Branches.Where(b => b.Code == code && !b.IsDeleted);

        if (orgId.HasValue && orgId.Value != Guid.Empty)
        {
            query = query.Where(b => b.OrganizationId == orgId.Value);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(List<Branch> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        string? city = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Branches
            .AsNoTracking()
            .Include(b => b.UserBranchAccesses.Where(uba => uba.IsActive))
                .ThenInclude(uba => uba.User)
            .Where(b => !b.IsDeleted && b.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(b => b.City == city);
        }

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: b => b.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }

    public async Task<List<Branch>> GetAllActiveByOrgAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Branches
            .AsNoTracking()
            .Where(b => !b.IsDeleted && b.OrganizationId == organizationId && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserBranchAccess>> GetUserBranchesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.UserBranchAccesses
            .AsNoTracking()
            .Include(uba => uba.Branch)
            .Where(uba => uba.UserId == userId && uba.IsActive && !uba.Branch.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task AssignUserBranchesAsync(Guid userId, List<Guid> branchIds, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        var existingAccesses = await _db.UserBranchAccesses
            .Where(uba => uba.UserId == userId)
            .ToListAsync(cancellationToken);

        _db.UserBranchAccesses.RemoveRange(existingAccesses);

        var distinctBranchIds = branchIds.Distinct().ToList();
        foreach (var branchId in distinctBranchIds)
        {
            await _db.UserBranchAccesses.AddAsync(new UserBranchAccess
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BranchId = branchId,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
    }
    #endregion

    #region Command Methods
    public async Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        await _db.Branches.AddAsync(branch, cancellationToken);
    }

    public void Update(Branch branch)
    {
        _db.Branches.Update(branch);
    }

    public void Delete(Branch branch)
    {
        branch.IsDeleted = true;
        branch.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
