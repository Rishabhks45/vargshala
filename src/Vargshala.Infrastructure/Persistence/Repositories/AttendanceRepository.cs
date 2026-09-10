using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;
using Vargshala.Infrastructure.Persistence;

namespace Vargshala.Infrastructure.Persistence.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly VargshalaDbContext _db;

    public AttendanceRepository(VargshalaDbContext db)
    {
        _db = db;
    }

    public async Task<ClassSession?> GetSessionWithDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _db.ClassSessions
            .AsNoTracking()
            .Include(s => s.Batch)
                .ThenInclude(b => b.Class)
                    .ThenInclude(c => c!.Branch)
            .Include(s => s.Batch)
                .ThenInclude(b => b.Subject)
            .Include(s => s.Teacher)
                .ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted, cancellationToken);
    }

    public async Task<List<BatchStudent>> GetEnrolledStudentsForBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await _db.BatchStudents
            .AsNoTracking()
            .Include(bs => bs.Student)
                .ThenInclude(s => s.User)
            .Where(bs => bs.BatchId == batchId && bs.IsActive)
            .OrderBy(bs => bs.Student.RollNumber)
            .ThenBy(bs => bs.Student.User.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Attendance>> GetAttendancesForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Where(a => a.ClassSessionId == sessionId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attendance?> GetAttendanceAsync(Guid sessionId, Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _db.Attendances
            .FirstOrDefaultAsync(a => a.ClassSessionId == sessionId && a.StudentId == studentId && !a.IsDeleted, cancellationToken);
    }

    public async Task<List<Attendance>> GetAttendancesForSessionsAsync(IEnumerable<Guid> sessionIds, CancellationToken cancellationToken = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Where(a => sessionIds.Contains(a.ClassSessionId) && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<BatchAttendanceOverviewDto?> GetBatchOverviewAsync(Guid batchId, DateOnly referenceDate, CancellationToken cancellationToken = default)
    {
        var batch = await _db.Batches
            .AsNoTracking()
            .Include(b => b.Class)
                .ThenInclude(c => c!.Branch)
            .Include(b => b.Subject)
            .Include(b => b.BatchSchedules)
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDeleted, cancellationToken);

        if (batch == null) return null;

        var totalStudents = await _db.BatchStudents
            .CountAsync(bs => bs.BatchId == batchId && bs.IsActive, cancellationToken);

        // Schedule string
        var schedules = batch.BatchSchedules.OrderBy(s => s.DayOfWeek).ToList();
        string scheduleFormatted = "";
        if (schedules.Count > 0)
        {
            var days = string.Join(", ", schedules.Select(s => s.DayOfWeek.GetShortName()));
            var first = schedules[0];
            scheduleFormatted = $"{days} | {first.StartTime:hh\\:mm tt} - {first.EndTime:hh\\:mm tt}";
        }
        else
        {
            scheduleFormatted = "Schedule not configured";
        }

        // Today's sessions on referenceDate
        var todaySessions = await _db.ClassSessions
            .AsNoTracking()
            .Where(cs => cs.BatchId == batchId && cs.SessionDate == referenceDate && !cs.IsDeleted)
            .ToListAsync(cancellationToken);

        var todaySessionIds = todaySessions.Select(s => s.Id).ToList();
        var todayAttendances = await _db.Attendances
            .AsNoTracking()
            .Where(a => todaySessionIds.Contains(a.ClassSessionId) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        int present = todayAttendances.Count(a => a.Status == AttendanceStatus.Present);
        int absent = todayAttendances.Count(a => a.Status == AttendanceStatus.Absent);
        int late = todayAttendances.Count(a => a.Status == AttendanceStatus.Late);
        int excused = todayAttendances.Count(a => a.Status == AttendanceStatus.Excused);

        // 7-day trend ending on referenceDate
        var startDate = referenceDate.AddDays(-6);
        var weekSessions = await _db.ClassSessions
            .AsNoTracking()
            .Where(cs => cs.BatchId == batchId && cs.SessionDate >= startDate && cs.SessionDate <= referenceDate && !cs.IsDeleted)
            .ToListAsync(cancellationToken);

        var weekSessionIds = weekSessions.Select(s => s.Id).ToList();
        var weekAttendances = await _db.Attendances
            .AsNoTracking()
            .Where(a => weekSessionIds.Contains(a.ClassSessionId) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        var trends = new List<DailyAttendanceTrendDto>();
        for (int i = 0; i < 7; i++)
        {
            var d = startDate.AddDays(i);
            var daySessionIds = weekSessions.Where(s => s.SessionDate == d).Select(s => s.Id).ToList();
            var dayAtts = weekAttendances.Where(a => daySessionIds.Contains(a.ClassSessionId)).ToList();
            int dayPresent = dayAtts.Count(a => a.Status == AttendanceStatus.Present);
            int dayLate = dayAtts.Count(a => a.Status == AttendanceStatus.Late);
            int dayAbsent = dayAtts.Count(a => a.Status == AttendanceStatus.Absent);
            int pct = 0;
            if (totalStudents > 0 && daySessionIds.Count > 0)
            {
                pct = (int)Math.Round((double)(dayPresent + dayLate) / (totalStudents * daySessionIds.Count) * 100);
            }

            var dow = d.DayOfWeek switch
            {
                System.DayOfWeek.Monday => "Mon",
                System.DayOfWeek.Tuesday => "Tue",
                System.DayOfWeek.Wednesday => "Wed",
                System.DayOfWeek.Thursday => "Thu",
                System.DayOfWeek.Friday => "Fri",
                System.DayOfWeek.Saturday => "Sat",
                _ => "Sun"
            };

            trends.Add(new DailyAttendanceTrendDto
            {
                Date = d,
                DayOfWeekShort = dow,
                DateShortFormatted = d.ToString("dd MMM"),
                TotalStudents = totalStudents,
                PresentCount = dayPresent,
                LateCount = dayLate,
                AbsentCount = dayAbsent,
                Percentage = pct
            });
        }

        // Recent marked activities
        var recentAtts = await _db.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.ClassSession)
            .Where(a => a.ClassSession.BatchId == batchId && !a.IsDeleted && a.MarkedAt.HasValue)
            .OrderByDescending(a => a.MarkedAt)
            .Take(6)
            .ToListAsync(cancellationToken);

        var recentList = recentAtts.Select(a => new RecentAttendanceActivityDto
        {
            StudentId = a.StudentId,
            StudentName = a.Student?.User != null ? $"{a.Student.User.FirstName} {a.Student.User.LastName}".Trim() : "Student",
            Status = a.Status,
            MarkedAt = a.MarkedAt,
            MarkedAtFormatted = a.Status == AttendanceStatus.Absent ? "-" : (a.MarkedAt.HasValue ? a.MarkedAt.Value.ToString("hh:mm tt") : "-")
        }).ToList();

        return new BatchAttendanceOverviewDto
        {
            BatchId = batch.Id,
            BatchName = batch.Name,
            ClassName = batch.Class?.Name ?? "Class",
            SubjectName = batch.Subject?.Name ?? "Subject",
            BranchName = batch.Class?.Branch?.Name ?? "Branch",
            ScheduleFormatted = scheduleFormatted,
            TotalStudents = totalStudents,
            TodayPresent = present,
            TodayAbsent = absent,
            TodayLate = late,
            TodayExcused = excused,
            DailyTrends = trends,
            RecentActivities = recentList
        };
    }

    public async Task<List<DateAttendanceReportDto>> GetDateWiseReportAsync(Guid batchId, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default)
    {
        var query = _db.ClassSessions
            .AsNoTracking()
            .Include(cs => cs.Teacher)
                .ThenInclude(t => t!.User)
            .Include(cs => cs.Batch)
            .Where(cs => cs.BatchId == batchId && !cs.IsDeleted);

        if (fromDate.HasValue) query = query.Where(cs => cs.SessionDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(cs => cs.SessionDate <= toDate.Value);

        var sessions = await query
            .OrderByDescending(cs => cs.SessionDate)
            .ThenByDescending(cs => cs.StartTime)
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var attendances = await _db.Attendances
            .AsNoTracking()
            .Where(a => sessionIds.Contains(a.ClassSessionId) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalEnrolled = await _db.BatchStudents
            .CountAsync(bs => bs.BatchId == batchId && bs.IsActive, cancellationToken);

        var list = new List<DateAttendanceReportDto>();
        foreach (var s in sessions)
        {
            var sessionAtts = attendances.Where(a => a.ClassSessionId == s.Id).ToList();
            list.Add(new DateAttendanceReportDto
            {
                SessionId = s.Id,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BatchName = s.Batch?.Name ?? "Batch",
                TeacherName = s.Teacher?.User != null ? $"{s.Teacher.User.FirstName} {s.Teacher.User.LastName}".Trim() : "Unassigned",
                Topic = s.Topic,
                TotalStudents = totalEnrolled,
                PresentCount = sessionAtts.Count(a => a.Status == AttendanceStatus.Present),
                AbsentCount = sessionAtts.Count(a => a.Status == AttendanceStatus.Absent),
                LateCount = sessionAtts.Count(a => a.Status == AttendanceStatus.Late),
                ExcusedCount = sessionAtts.Count(a => a.Status == AttendanceStatus.Excused)
            });
        }
        return list;
    }

    public async Task<List<StudentAttendanceReportDto>> GetStudentWiseReportAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var batchStudents = await _db.BatchStudents
            .AsNoTracking()
            .Include(bs => bs.Student)
                .ThenInclude(s => s.User)
            .Where(bs => bs.BatchId == batchId && bs.IsActive)
            .OrderBy(bs => bs.Student.RollNumber)
            .ThenBy(bs => bs.Student.User.FirstName)
            .ToListAsync(cancellationToken);

        var sessions = await _db.ClassSessions
            .AsNoTracking()
            .Where(cs => cs.BatchId == batchId && !cs.IsDeleted)
            .ToListAsync(cancellationToken);

        int totalSessions = sessions.Count;
        var sessionIds = sessions.Select(s => s.Id).ToList();

        var attendances = await _db.Attendances
            .AsNoTracking()
            .Where(a => sessionIds.Contains(a.ClassSessionId) && !a.IsDeleted)
            .ToListAsync(cancellationToken);

        var list = new List<StudentAttendanceReportDto>();
        foreach (var bs in batchStudents)
        {
            var student = bs.Student;
            if (student == null) continue;

            var studentAtts = attendances.Where(a => a.StudentId == student.Id).ToList();
            int pres = studentAtts.Count(a => a.Status == AttendanceStatus.Present);
            int abs = studentAtts.Count(a => a.Status == AttendanceStatus.Absent);
            int late = studentAtts.Count(a => a.Status == AttendanceStatus.Late);
            int exc = studentAtts.Count(a => a.Status == AttendanceStatus.Excused);

            list.Add(new StudentAttendanceReportDto
            {
                StudentId = student.Id,
                FullName = student.User != null ? $"{student.User.FirstName} {student.User.LastName}".Trim() : "Student",
                RollNumber = student.RollNumber,
                StudentCode = student.StudentCode,
                TotalClasses = totalSessions,
                PresentCount = pres,
                AbsentCount = abs,
                LateCount = late,
                ExcusedCount = exc
            });
        }
        return list;
    }

    public async Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        await _db.Attendances.AddAsync(attendance, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Attendance> attendances, CancellationToken cancellationToken = default)
    {
        await _db.Attendances.AddRangeAsync(attendances, cancellationToken);
    }

    public void Update(Attendance attendance)
    {
        _db.Attendances.Update(attendance);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }
}
