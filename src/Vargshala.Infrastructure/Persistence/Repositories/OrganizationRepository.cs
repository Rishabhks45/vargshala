using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    #region Fields & Constructor
    private readonly IVargshalaDbContext _db;

    public OrganizationRepository(IVargshalaDbContext db)
    {
        _db = db;
    }
    #endregion

    #region Search & Sort Mappings
    // ---- Searchable fields for Organization ----
    private static Func<string, Expression<Func<Organization, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return o => (o.Name != null && EF.Functions.Like(o.Name.ToLower(), lowerTerm))
          || (o.Code != null && EF.Functions.Like(o.Code.ToLower(), lowerTerm))
          || (o.Email != null && EF.Functions.Like(o.Email.ToLower(), lowerTerm))
          || (o.City != null && EF.Functions.Like(o.City.ToLower(), lowerTerm))
          || (o.Mobile != null && EF.Functions.Like(o.Mobile, $"%{term}%"));
    };

    // ---- Sortable fields whitelist for Organization ----
    private static readonly Dictionary<string, Expression<Func<Organization, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = o => o.Name,
        ["code"] = o => o.Code,
        ["email"] = o => o.Email!,
        ["city"] = o => o.City!,
        ["state"] = o => o.State!,
        ["isactive"] = o => o.IsActive,
        ["status"] = o => o.IsActive,
        ["createdat"] = o => o.CreatedAt,
        ["owner"] = o => o.Users.Where(u => u.Role == UserRole.OrganizationAdmin && !u.IsDeleted).Select(u => u.FirstName).FirstOrDefault() ?? "",
        ["students"] = o => o.Users.Count(u => u.Role == UserRole.Student && !u.IsDeleted),
        ["plan"] = o => o.Code,
    };
    #endregion

    #region Query Methods
    public async Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Organization> Items, int TotalRecords)> GetPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Organizations
            .AsNoTracking()
            .Where(o => !o.IsDeleted);

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: o => o.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }

    public async Task<(List<InstituteSummaryDto> Items, int TotalRecords)> GetInstitutesSummaryPagedAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Get paginated organizations
        var (organizations, totalRecords) = await GetPagedAsync(request, cancellationToken);
        if (organizations.Count == 0)
        {
            return (new List<InstituteSummaryDto>(), totalRecords);
        }

        var orgIds = organizations.Select(o => o.Id).ToList();

        // 2. Fetch user counts grouped by organization and role
        var orgUserCounts = await _db.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId.HasValue && orgIds.Contains(u.OrganizationId.Value) && !u.IsDeleted)
            .GroupBy(u => new { u.OrganizationId, u.Role })
            .Select(g => new { g.Key.OrganizationId, g.Key.Role, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // 3. Fetch admin names for organizations
        var orgAdmins = await _db.Users
            .AsNoTracking()
            .Where(u => u.OrganizationId.HasValue
                     && orgIds.Contains(u.OrganizationId.Value)
                     && u.Role == UserRole.OrganizationAdmin
                     && !u.IsDeleted)
            .Select(u => new { u.OrganizationId, u.FirstName, u.LastName, u.Email })
            .ToListAsync(cancellationToken);

        var list = new List<InstituteSummaryDto>(organizations.Count);

        // 4. Map to InstituteSummaryDto
        foreach (var org in organizations)
        {
            var counts = orgUserCounts.Where(c => c.OrganizationId == org.Id).ToList();
            var studentCount = counts.Where(c => c.Role == UserRole.Student).Sum(c => c.Count);
            var teacherCount = counts.Where(c => c.Role == UserRole.Teacher).Sum(c => c.Count);
            var totalUsers = counts.Sum(c => c.Count);

            var admin = orgAdmins.FirstOrDefault(a => a.OrganizationId == org.Id);

            var plan = org.Code.Equals("VARGSHALA", StringComparison.OrdinalIgnoreCase)
                ? "Enterprise"
                : (studentCount > 100 ? "Pro Institute" : "Standard");

            list.Add(new InstituteSummaryDto
            {
                Id = org.Id,
                Name = org.Name,
                Code = org.Code,
                OwnerName = admin != null ? $"{admin.FirstName} {admin.LastName}".Trim() : "Administrator",
                Email = org.Email ?? admin?.Email,
                Mobile = org.Mobile,
                Address = org.Address,
                City = org.City ?? "Not Specified",
                State = org.State ?? "India",
                Pincode = org.Pincode,
                LogoUrl = org.LogoUrl,
                AcademicSession = org.AcademicSession,
                StudentCount = studentCount,
                TeacherCount = teacherCount,
                TotalUsersCount = totalUsers,
                Plan = plan,
                IsActive = org.IsActive,
                CreatedAt = org.CreatedAt
            });
        }

        return (list, totalRecords);
    }

    public async Task<Organization?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _db.Organizations
            .AnyAsync(o => o.Code == code && !o.IsDeleted, cancellationToken);
    }
    #endregion

    #region Command Methods
    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        await _db.Organizations.AddAsync(organization, cancellationToken);
    }

    public void Update(Organization organization)
    {
        _db.Organizations.Update(organization);
    }

    public void Delete(Organization organization)
    {
        organization.IsDeleted = true;
        organization.DeletedAt = DateTime.UtcNow;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
