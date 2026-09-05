using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly IVargshalaDbContext _db;
    private readonly ICurrentUser _currentUser;

    public StudentRepository(IVargshalaDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<Student, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return s => (s.User != null && s.User.FirstName != null && EF.Functions.Like(s.User.FirstName.ToLower(), lowerTerm))
          || (s.User != null && s.User.LastName != null && EF.Functions.Like(s.User.LastName.ToLower(), lowerTerm))
          || (s.User != null && s.User.Email != null && EF.Functions.Like(s.User.Email.ToLower(), lowerTerm))
          || (s.User != null && s.User.Mobile != null && EF.Functions.Like(s.User.Mobile, $"%{term}%"))
          || (s.StudentCode != null && EF.Functions.Like(s.StudentCode.ToLower(), lowerTerm))
          || (s.RollNumber != null && EF.Functions.Like(s.RollNumber.ToLower(), lowerTerm))
          || (s.ClassName != null && EF.Functions.Like(s.ClassName.ToLower(), lowerTerm))
          || (s.FatherName != null && EF.Functions.Like(s.FatherName.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<Student, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = s => s.User.FirstName,
        ["firstname"] = s => s.User.FirstName,
        ["lastname"] = s => s.User.LastName,
        ["email"] = s => s.User.Email!,
        ["phone"] = s => s.User.Mobile!,
        ["mobile"] = s => s.User.Mobile!,
        ["studentcode"] = s => s.StudentCode!,
        ["enrollmentnumber"] = s => s.StudentCode!,
        ["classname"] = s => s.ClassName!,
        ["section"] = s => s.Section!,
        ["rollnumber"] = s => s.RollNumber!,
        ["fathername"] = s => s.FatherName!,
        ["parentname"] = s => s.FatherName!,
        ["isactive"] = s => s.IsActive,
        ["status"] = s => s.IsActive,
        ["admissiondate"] = s => s.AdmissionDate!,
        ["createdat"] = s => s.CreatedAt,
    };
    #endregion

    #region Query Methods
    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<Student?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<Student?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<Student?> GetByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default)
    {
        return await _db.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode && !s.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByStudentCodeAsync(string studentCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var orgId = _currentUser.OrganizationId;
        var query = _db.Students.Where(s => s.StudentCode == studentCode && !s.IsDeleted);

        if (orgId.HasValue && orgId.Value != Guid.Empty)
        {
            query = query.Where(s => s.User != null && s.User.OrganizationId == orgId.Value);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(List<Student> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        string? className = null,
        string? section = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Students
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => !s.IsDeleted && s.User.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(className))
        {
            query = query.Where(s => s.ClassName == className);
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            query = query.Where(s => s.Section == section);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: s => s.CreatedAt,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }
    #endregion

    #region Command Methods
    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _db.Students.AddAsync(student, cancellationToken);
    }

    public void Update(Student student)
    {
        _db.Students.Update(student);
    }

    public void Delete(Student student)
    {
        student.IsDeleted = true;
        student.DeletedAt = DateTime.UtcNow;
        if (student.User != null)
        {
            student.User.IsDeleted = true;
            student.User.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
