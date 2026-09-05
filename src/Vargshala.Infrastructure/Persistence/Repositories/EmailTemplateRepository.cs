using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.Emails.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class EmailTemplateRepository : IEmailTemplateRepository
{
    #region Fields & Constructor
    private readonly IVargshalaDbContext _db;

    public EmailTemplateRepository(IVargshalaDbContext db)
    {
        _db = db;
    }
    #endregion

    #region Search & Sort Mappings
    // ---- Searchable fields for EmailTemplate ----
    private static Func<string, Expression<Func<EmailTemplate, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return e => EF.Functions.Like(e.Name.ToLower(), lowerTerm)
                 || EF.Functions.Like(e.Code.ToLower(), lowerTerm)
                 || EF.Functions.Like(e.Subject.ToLower(), lowerTerm);
    };

    // ---- Sortable fields whitelist for EmailTemplate ----
    private static readonly Dictionary<string, Expression<Func<EmailTemplate, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = e => e.Name,
        ["category"] = e => e.Category,
        ["role"] = e => e.TargetRole ?? (object)0,
        ["status"] = e => e.IsActive,
        ["isactive"] = e => e.IsActive,
        ["code"] = e => e.Code,
        ["createdat"] = e => e.CreatedAt,
    };
    #endregion

    #region Query Methods
    public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.EmailTemplates
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _db.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Code == normalizedCode && !e.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var query = _db.EmailTemplates.Where(e => e.Code == normalizedCode && !e.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(List<EmailTemplate> Items, int TotalRecords)> GetTemplatesPagedAsync(
        PagedRequest request,
        EmailTemplateCategory? category = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.EmailTemplates
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        if (category.HasValue)
        {
            query = query.Where(e => e.Category == category.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: e => e.Name,
            defaultAscending: true,
            cancellationToken: cancellationToken);
    }
    #endregion

    #region Command Methods
    public async Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        await _db.EmailTemplates.AddAsync(template, cancellationToken);
    }

    public void Update(EmailTemplate template)
    {
        _db.EmailTemplates.Update(template);
    }

    public void Delete(EmailTemplate template)
    {
        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
