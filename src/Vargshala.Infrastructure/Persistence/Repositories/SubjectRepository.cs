using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.Subjects.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly IVargshalaDbContext _db;

    public SubjectRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<Subject, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return s => (s.Name != null && EF.Functions.Like(s.Name.ToLower(), lowerTerm))
                 || (s.Code != null && EF.Functions.Like(s.Code.ToLower(), lowerTerm))
                 || (s.Description != null && EF.Functions.Like(s.Description.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<Subject, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = s => s.Name,
        ["code"] = s => s.Code,
        ["isactive"] = s => s.IsActive,
        ["createdat"] = s => s.CreatedAt,
        ["updatedat"] = s => s.UpdatedAt!
    };
    #endregion

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<Subject?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Subjects
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAndOrgAsync(string code, Guid organizationId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Subjects
            .Where(s => s.OrganizationId == organizationId && s.Code.ToLower() == code.ToLower() && !s.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(List<Subject> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Subjects
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId && !s.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: s => s.Name,
            defaultAscending: true,
            cancellationToken: cancellationToken);
    }

    public async Task<List<Subject>> GetAllActiveByOrgAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _db.Subjects
            .AsNoTracking()
            .Where(s => s.OrganizationId == organizationId && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        await _db.Subjects.AddAsync(subject, cancellationToken);
    }

    public void Update(Subject subject)
    {
        _db.Subjects.Update(subject);
    }

    public void Delete(Subject subject)
    {
        subject.IsDeleted = true;
        _db.Subjects.Update(subject);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
