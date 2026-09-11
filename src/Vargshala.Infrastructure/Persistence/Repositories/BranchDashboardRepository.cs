using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.BranchAdmin.Dashboard.Infrastructure;
using Vargshala.Contracts.BranchAdmin;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class BranchDashboardRepository : IBranchDashboardRepository
{
    private readonly IVargshalaDbContext _db;

    public BranchDashboardRepository(IVargshalaDbContext db)
    {
        _db = db;
    }

    public async Task<BranchDashboardDto?> GetDashboardStatsAsync(Guid organizationId, Guid branchId, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == branchId && b.OrganizationId == organizationId && !b.IsDeleted, cancellationToken);

        if (branch == null)
            return null;

        var totalStudents = await _db.BatchStudents
            .AsNoTracking()
            .Where(bs => bs.IsActive && bs.Batch.Class.BranchId == branchId && !bs.Batch.IsDeleted && !bs.Batch.Class.IsDeleted)
            .Select(bs => bs.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalTeachers = await _db.Teachers
            .AsNoTracking()
            .Where(t => !t.IsDeleted && (
                t.User.UserBranchAccesses.Any(uba => uba.BranchId == branchId && uba.IsActive)
                || !t.User.UserBranchAccesses.Any(uba => uba.IsActive)
                || t.BatchTeachers.Any(bt => bt.Batch.Class.BranchId == branchId && !bt.Batch.IsDeleted)))
            .CountAsync(cancellationToken);

        var totalClasses = await _db.Classes
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && !c.IsDeleted)
            .CountAsync(cancellationToken);

        var totalBatches = await _db.Batches
            .AsNoTracking()
            .Where(b => b.Class.BranchId == branchId && !b.IsDeleted)
            .CountAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todaySessionsCount = await _db.ClassSessions
            .AsNoTracking()
            .Where(cs => cs.Batch.Class.BranchId == branchId && cs.SessionDate == today && !cs.IsDeleted)
            .CountAsync(cancellationToken);

        var todayAttendanceQuery = _db.Attendances
            .AsNoTracking()
            .Where(a => a.ClassSession.Batch.Class.BranchId == branchId && a.ClassSession.SessionDate == today && !a.IsDeleted);

        var totalAttendances = await todayAttendanceQuery.CountAsync(cancellationToken);
        var presentAttendances = await todayAttendanceQuery.Where(a => a.Status == "Present").CountAsync(cancellationToken);

        var attendancePercentage = totalAttendances > 0
            ? Math.Round((double)presentAttendances / totalAttendances * 100, 1)
            : 0;

        var recentSessionsEntities = await _db.ClassSessions
            .AsNoTracking()
            .Include(cs => cs.Batch).ThenInclude(b => b.Class)
            .Include(cs => cs.Batch).ThenInclude(b => b.Subject)
            .Include(cs => cs.Teacher).ThenInclude(t => t!.User)
            .Where(cs => cs.Batch.Class.BranchId == branchId && !cs.IsDeleted)
            .OrderByDescending(cs => cs.SessionDate)
            .ThenByDescending(cs => cs.StartTime)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentSessions = recentSessionsEntities.Select(s => new BranchRecentSessionDto
        {
            Id = s.Id,
            BatchName = s.Batch?.Name ?? "General",
            ClassName = s.Batch?.Class?.Name ?? "General",
            SubjectName = s.Batch?.Subject?.Name ?? "-",
            TeacherName = s.Teacher?.User != null ? $"{s.Teacher.User.FirstName} {s.Teacher.User.LastName}".Trim() : "-",
            Date = s.SessionDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Status = s.Status
        }).ToList();

        return new BranchDashboardDto
        {
            BranchId = branch.Id,
            BranchName = branch.Name,
            BranchCode = branch.Code,
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalClasses = totalClasses,
            TotalBatches = totalBatches,
            TodaySessionsCount = todaySessionsCount,
            TodayAttendancePercentage = attendancePercentage,
            RecentSessions = recentSessions
        };
    }
}
