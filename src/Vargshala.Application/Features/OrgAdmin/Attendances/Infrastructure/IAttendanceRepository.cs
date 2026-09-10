using Vargshala.Contracts.Attendances;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;

public interface IAttendanceRepository
{
    Task<ClassSession?> GetSessionWithDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<List<BatchStudent>> GetEnrolledStudentsForBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<List<Attendance>> GetAttendancesForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Attendance?> GetAttendanceAsync(Guid sessionId, Guid studentId, CancellationToken cancellationToken = default);
    Task<List<Attendance>> GetAttendancesForSessionsAsync(IEnumerable<Guid> sessionIds, CancellationToken cancellationToken = default);
    Task<BatchAttendanceOverviewDto?> GetBatchOverviewAsync(Guid batchId, DateOnly referenceDate, CancellationToken cancellationToken = default);
    Task<List<DateAttendanceReportDto>> GetDateWiseReportAsync(Guid batchId, DateOnly? fromDate, DateOnly? toDate, CancellationToken cancellationToken = default);
    Task<List<StudentAttendanceReportDto>> GetStudentWiseReportAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Attendance> attendances, CancellationToken cancellationToken = default);
    void Update(Attendance attendance);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
