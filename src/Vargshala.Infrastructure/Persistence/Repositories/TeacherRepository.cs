using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public TeacherRepository(IVargshalaDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<Teacher, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return t => (t.User != null && t.User.FirstName != null && EF.Functions.Like(t.User.FirstName.ToLower(), lowerTerm))
          || (t.User != null && t.User.LastName != null && EF.Functions.Like(t.User.LastName.ToLower(), lowerTerm))
          || (t.User != null && t.User.Email != null && EF.Functions.Like(t.User.Email.ToLower(), lowerTerm))
          || (t.User != null && t.User.Mobile != null && EF.Functions.Like(t.User.Mobile, $"%{term}%"))
          || (t.EmployeeCode != null && EF.Functions.Like(t.EmployeeCode.ToLower(), lowerTerm))
          || (t.Department != null && EF.Functions.Like(t.Department.ToLower(), lowerTerm))
          || (t.Designation != null && EF.Functions.Like(t.Designation.ToLower(), lowerTerm))
          || (t.Specialization != null && EF.Functions.Like(t.Specialization.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<Teacher, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = t => t.User.FirstName,
        ["firstname"] = t => t.User.FirstName,
        ["lastname"] = t => t.User.LastName,
        ["email"] = t => t.User.Email!,
        ["phone"] = t => t.User.Mobile!,
        ["mobile"] = t => t.User.Mobile!,
        ["employeecode"] = t => t.EmployeeCode!,
        ["employeeid"] = t => t.EmployeeCode!,
        ["department"] = t => t.Department!,
        ["designation"] = t => t.Designation!,
        ["qualification"] = t => t.HighestQualification!,
        ["highestqualification"] = t => t.HighestQualification!,
        ["subject"] = t => t.Specialization!,
        ["specialization"] = t => t.Specialization!,
        ["isactive"] = t => t.IsActive,
        ["status"] = t => t.IsActive,
        ["joiningdate"] = t => t.JoiningDate!,
        ["createdat"] = t => t.CreatedAt,
    };
    #endregion

    #region Query Methods
    public async Task<Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<Teacher?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<Teacher?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
    {
        return await _db.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.EmployeeCode == employeeCode && !t.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByEmployeeCodeAsync(string employeeCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var orgId = _currentUser.OrganizationId;
        var query = _db.Teachers.Where(t => t.EmployeeCode == employeeCode && !t.IsDeleted);

        if (orgId.HasValue && orgId.Value != Guid.Empty)
        {
            query = query.Where(t => t.User != null && t.User.OrganizationId == orgId.Value);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(List<Teacher> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        string? department = null,
        string? designation = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => !t.IsDeleted && t.User.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(t => t.Department == department);
        }

        if (!string.IsNullOrWhiteSpace(designation))
        {
            query = query.Where(t => t.Designation == designation);
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: t => t.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }
    #endregion

    #region Command Methods
    public async Task AddAsync(Teacher teacher, CancellationToken cancellationToken = default)
    {
        await _db.Teachers.AddAsync(teacher, cancellationToken);
    }

    public void Update(Teacher teacher)
    {
        _db.Teachers.Update(teacher);
    }

    public void Delete(Teacher teacher)
    {
        teacher.IsDeleted = true;
        teacher.DeletedAt = DateTime.UtcNow;
        if (teacher.User != null)
        {
            teacher.User.IsDeleted = true;
            teacher.User.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
