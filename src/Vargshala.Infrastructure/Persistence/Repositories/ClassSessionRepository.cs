using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Common;
using Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class ClassSessionRepository : IClassSessionRepository
{
    private readonly IVargshalaDbContext _db;

    public ClassSessionRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    #region Search & Sort Mappings
    private static Func<string, Expression<Func<ClassSession, bool>>> SearchPredicate => term =>
    {
        var lowerTerm = $"%{term.ToLower()}%";
        return s => (s.Topic != null && EF.Functions.Like(s.Topic.ToLower(), lowerTerm))
                 || (s.Notes != null && EF.Functions.Like(s.Notes.ToLower(), lowerTerm))
                 || (s.Batch != null && s.Batch.Name != null && EF.Functions.Like(s.Batch.Name.ToLower(), lowerTerm))
                 || (s.Batch != null && s.Batch.Code != null && EF.Functions.Like(s.Batch.Code.ToLower(), lowerTerm))
                 || (s.Teacher != null && s.Teacher.User != null && s.Teacher.User.FirstName != null && EF.Functions.Like(s.Teacher.User.FirstName.ToLower(), lowerTerm));
    };

    private static readonly Dictionary<string, Expression<Func<ClassSession, object>>> SortMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sessiondate"] = s => s.SessionDate,
        ["date"] = s => s.SessionDate,
        ["starttime"] = s => s.StartTime,
        ["timing"] = s => s.StartTime,
        ["time"] = s => s.StartTime,
        ["batch"] = s => s.Batch!.Name,
        ["batchname"] = s => s.Batch!.Name,
        ["class"] = s => s.Batch!.Class!.Name,
        ["classname"] = s => s.Batch!.Class!.Name,
        ["subject"] = s => s.Batch!.Subject!.Name,
        ["subjectname"] = s => s.Batch!.Subject!.Name,
        ["branch"] = s => s.Batch!.Class!.Branch!.Name,
        ["branchname"] = s => s.Batch!.Class!.Branch!.Name,
        ["teacher"] = s => s.Teacher!.User!.FirstName,
        ["teachername"] = s => s.Teacher!.User!.FirstName,
        ["faculty"] = s => s.Teacher!.User!.FirstName,
        ["topic"] = s => s.Topic!,
        ["status"] = s => s.Status,
        ["createdat"] = s => s.CreatedAt,
        ["updatedat"] = s => s.UpdatedAt!
    };
    #endregion

    public async Task<(List<ClassSession> Items, int TotalRecords)> GetPagedAsync(
        Guid orgId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        Guid? teacherId = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ClassSessions
            .AsNoTracking()
            .Include(s => s.Batch)
                .ThenInclude(b => b!.Class)
                    .ThenInclude(c => c!.Branch)
            .Include(s => s.Batch)
                .ThenInclude(b => b!.Subject)
            .Include(s => s.Teacher)
                .ThenInclude(t => t!.User)
            .Include(s => s.BatchSchedule)
            .Where(s => s.Batch.Class.Branch.OrganizationId == orgId && !s.IsDeleted);

        if (branchId.HasValue && branchId.Value != Guid.Empty)
            query = query.Where(s => s.Batch.Class.BranchId == branchId.Value);

        if (classId.HasValue && classId.Value != Guid.Empty)
            query = query.Where(s => s.Batch.ClassId == classId.Value);

        if (batchId.HasValue && batchId.Value != Guid.Empty)
            query = query.Where(s => s.BatchId == batchId.Value);

        if (teacherId.HasValue && teacherId.Value != Guid.Empty)
            query = query.Where(s => s.TeacherId == teacherId.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.SessionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.SessionDate <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status.ToLower() == status.ToLower());

        return await query.ToPagedResultAsync(
            request,
            searchPredicate: SearchPredicate,
            sortMappings: SortMappings,
            defaultSortExpression: s => s.SessionDate,
            defaultAscending: false,
            cancellationToken: cancellationToken);
    }

    public async Task<ClassSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ClassSessions
            .AsNoTracking()
            .Include(s => s.Batch)
                .ThenInclude(b => b!.Class)
                    .ThenInclude(c => c!.Branch)
            .Include(s => s.Batch)
                .ThenInclude(b => b!.Subject)
            .Include(s => s.Teacher)
                .ThenInclude(t => t!.User)
            .Include(s => s.BatchSchedule)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<ClassSession?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ClassSessions
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid batchId, Guid? scheduleId, DateOnly sessionDate, CancellationToken cancellationToken = default)
    {
        return await _db.ClassSessions
            .AnyAsync(s => s.BatchId == batchId &&
                           s.BatchScheduleId == scheduleId &&
                           s.SessionDate == sessionDate &&
                           !s.IsDeleted, cancellationToken);
    }

    public async Task<List<BatchSchedule>> GetSchedulesForBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchSchedules
            .AsNoTracking()
            .Where(s => s.BatchId == batchId && s.IsActive && !s.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Teacher>> GetAssignedTeachersForBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchTeachers
            .AsNoTracking()
            .Where(bt => bt.BatchId == batchId && bt.IsActive)
            .Include(bt => bt.Teacher)
                .ThenInclude(t => t!.User)
            .Select(bt => bt.Teacher!)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ClassSession session, CancellationToken cancellationToken = default)
    {
        await _db.ClassSessions.AddAsync(session, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<ClassSession> sessions, CancellationToken cancellationToken = default)
    {
        await _db.ClassSessions.AddRangeAsync(sessions, cancellationToken);
    }

    public Task UpdateAsync(ClassSession session, CancellationToken cancellationToken = default)
    {
        _db.ClassSessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
