using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class BatchRepository : IBatchRepository
{
    private readonly IVargshalaDbContext _db;

    public BatchRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<Batch, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return b => (b.Name != null && EF.Functions.Like(b.Name.ToLower(), lowerTerm))
                 || (b.Code != null && EF.Functions.Like(b.Code.ToLower(), lowerTerm))
                 || (b.Class != null && b.Class.Name != null && EF.Functions.Like(b.Class.Name.ToLower(), lowerTerm))
                 || (b.Subject != null && b.Subject.Name != null && EF.Functions.Like(b.Subject.Name.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<Batch, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = b => b.Name,
        ["batchname"] = b => b.Name,
        ["code"] = b => b.Code,
        ["batchcode"] = b => b.Code,
        ["class"] = b => b.Class!.Name,
        ["classname"] = b => b.Class!.Name,
        ["subject"] = b => b.Subject!.Name,
        ["subjectname"] = b => b.Subject!.Name,
        ["branch"] = b => b.Class!.Branch!.Name,
        ["branchname"] = b => b.Class!.Branch!.Name,
        ["isactive"] = b => b.IsActive,
        ["status"] = b => b.IsActive,
        ["createdat"] = b => b.CreatedAt,
        ["updatedat"] = b => b.UpdatedAt!
    };
    #endregion

    public async Task<Batch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Batches
            .AsNoTracking()
            .Include(b => b.Class)
                .ThenInclude(c => c!.Branch)
            .Include(b => b.Subject)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Teacher)
                    .ThenInclude(t => t!.User)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Subject)
            .Include(b => b.BatchStudents)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<Batch?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Batches
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<Batch?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Batches
            .AsNoTracking()
            .Include(b => b.Class)
                .ThenInclude(c => c!.Branch)
            .Include(b => b.Subject)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Teacher)
                    .ThenInclude(t => t!.User)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Subject)
            .Include(b => b.BatchStudents)
                .ThenInclude(bs => bs.Student)
                    .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAndClassAsync(string code, Guid classId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Batches
            .Where(b => b.ClassId == classId && b.Code.ToLower() == code.ToLower() && !b.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(List<Batch> Items, int TotalRecords)> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? subjectId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Batches
            .AsNoTracking()
            .Include(b => b.Class)
                .ThenInclude(c => c!.Branch)
            .Include(b => b.Subject)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Teacher)
                    .ThenInclude(t => t!.User)
            .Include(b => b.BatchTeachers)
                .ThenInclude(bt => bt.Subject)
            .Include(b => b.BatchStudents)
            .Where(b => b.Class!.Branch!.OrganizationId == organizationId && !b.IsDeleted);

        if (branchId.HasValue)
        {
            query = query.Where(b => b.Class!.BranchId == branchId.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(b => b.ClassId == classId.Value);
        }

        if (subjectId.HasValue)
        {
            query = query.Where(b => b.SubjectId == subjectId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: b => b.Name,
            defaultAscending: true,
            cancellationToken: cancellationToken);
    }

    public async Task<List<Batch>> GetAllActiveAsync(Guid organizationId, Guid? classId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Batches
            .AsNoTracking()
            .Include(b => b.Class)
            .Include(b => b.Subject)
            .Where(b => b.Class!.Branch!.OrganizationId == organizationId && b.IsActive && !b.IsDeleted);

        if (classId.HasValue)
        {
            query = query.Where(b => b.ClassId == classId.Value);
        }

        return await query
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        await _db.Batches.AddAsync(batch, cancellationToken);
    }

    public void Update(Batch batch)
    {
        _db.Batches.Update(batch);
    }

    public void Delete(Batch batch)
    {
        batch.IsDeleted = true;
        _db.Batches.Update(batch);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }

    #region Batch Teachers
    public async Task<List<BatchTeacher>> GetTeachersByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchTeachers
            .AsNoTracking()
            .Include(bt => bt.Subject)
            .Include(bt => bt.Teacher)
                .ThenInclude(t => t!.User)
            .Where(bt => bt.BatchId == batchId && bt.IsActive)
            .OrderBy(bt => bt.Teacher!.User!.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<BatchTeacher?> GetBatchTeacherAsync(Guid batchId, Guid teacherId, Guid? subjectId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.BatchTeachers
            .Include(bt => bt.Subject)
            .Include(bt => bt.Teacher)
                .ThenInclude(t => t!.User)
            .Where(bt => bt.BatchId == batchId && bt.TeacherId == teacherId);

        if (subjectId.HasValue)
        {
            query = query.Where(bt => bt.SubjectId == subjectId.Value);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<BatchTeacher>> GetBatchTeachersByTeacherAsync(Guid batchId, Guid teacherId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchTeachers
            .Include(bt => bt.Subject)
            .Include(bt => bt.Teacher)
                .ThenInclude(t => t!.User)
            .Where(bt => bt.BatchId == batchId && bt.TeacherId == teacherId && bt.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddTeacherAsync(BatchTeacher batchTeacher, CancellationToken cancellationToken = default)
    {
        await _db.BatchTeachers.AddAsync(batchTeacher, cancellationToken);
    }

    public void UpdateTeacher(BatchTeacher batchTeacher)
    {
        _db.BatchTeachers.Update(batchTeacher);
    }
    #endregion

    #region Batch Students
    public async Task<List<BatchStudent>> GetStudentsByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchStudents
            .AsNoTracking()
            .Include(bs => bs.Student)
                .ThenInclude(s => s!.User)
            .Where(bs => bs.BatchId == batchId && bs.IsActive)
            .OrderBy(bs => bs.Student!.User!.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<BatchStudent?> GetBatchStudentAsync(Guid batchId, Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchStudents
            .FirstOrDefaultAsync(bs => bs.BatchId == batchId && bs.StudentId == studentId, cancellationToken);
    }

    public async Task<List<BatchStudent>> GetBatchesByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchStudents
            .AsNoTracking()
            .Include(bs => bs.Batch)
                .ThenInclude(b => b.Class)
                    .ThenInclude(c => c.Branch)
            .Include(bs => bs.Batch)
                .ThenInclude(b => b.Subject)
            .Where(bs => bs.StudentId == studentId && !bs.Batch.IsDeleted)
            .OrderByDescending(bs => bs.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddStudentAsync(BatchStudent batchStudent, CancellationToken cancellationToken = default)
    {
        await _db.BatchStudents.AddAsync(batchStudent, cancellationToken);
    }

    public void UpdateStudent(BatchStudent batchStudent)
    {
        _db.BatchStudents.Update(batchStudent);
    }
    #endregion
}
