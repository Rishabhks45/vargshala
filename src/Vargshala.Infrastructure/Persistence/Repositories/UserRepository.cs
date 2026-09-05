using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.Users.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    #region Fields & Constructor
    private readonly IVargshalaDbContext _db;

    public UserRepository(IVargshalaDbContext db)
    {
        _db = db;
    }
    #endregion

    #region Search & Sort Mappings
    // ---- Searchable fields for User ----
    private static Func<string, Expression<Func<User, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return u => (u.FirstName != null && EF.Functions.Like(u.FirstName.ToLower(), lowerTerm))
          || (u.LastName != null && EF.Functions.Like(u.LastName.ToLower(), lowerTerm))
          || (u.Email != null && EF.Functions.Like(u.Email.ToLower(), lowerTerm))
          || (u.Mobile != null && EF.Functions.Like(u.Mobile, $"%{term}%"))
          || (u.Organization != null && EF.Functions.Like(u.Organization.Name.ToLower(), lowerTerm));
    };

    // ---- Sortable fields whitelist for User ----
    private static readonly Dictionary<string, Expression<Func<User, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = u => u.FirstName,
        ["firstname"] = u => u.FirstName,
        ["lastname"] = u => u.LastName,
        ["email"] = u => u.Email!,
        ["mobile"] = u => u.Mobile!,
        ["role"] = u => u.Role,
        ["isactive"] = u => u.IsActive,
        ["status"] = u => u.IsActive,
        ["createdat"] = u => u.CreatedAt,
        ["lastloginat"] = u => u.LastLoginAt!,
        ["organization"] = u => u.Organization != null ? u.Organization.Name : "",
    };
    #endregion

    #region Query Methods
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByIdWithOrgAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.OrganizationId == organizationId && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAndOrgAsync(string email, Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AnyAsync(u => u.Email == email && u.OrganizationId == organizationId && !u.IsDeleted, cancellationToken);
    }

    public async Task<(List<User> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId == organizationId && !u.IsDeleted);

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: u => u.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }

    public async Task<(List<User> Items, int TotalRecords)> GetControlPanelUsersPagedAsync(
        PagedRequest request,
        UserRole? role = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .Include(u => u.Organization)
            .Where(u => !u.IsDeleted && (u.Role == UserRole.SuperAdmin || u.Role == UserRole.BackOffice || u.Role == UserRole.OrganizationAdmin));

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: u => u.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }
    #endregion

    #region Command Methods
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _db.Users.Update(user);
    }

    public void Delete(User user)
    {
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
