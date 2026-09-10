using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions.Infrastructure;

public interface IClassSessionRepository
{
    Task<(List<ClassSession> Items, int TotalRecords)> GetPagedAsync(
        Guid orgId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? batchId = null,
        Guid? teacherId = null,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<ClassSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassSession?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid batchId, Guid? scheduleId, DateOnly sessionDate, CancellationToken cancellationToken = default);
    Task<List<BatchSchedule>> GetSchedulesForBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<List<Teacher>> GetAssignedTeachersForBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task AddAsync(ClassSession session, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ClassSession> sessions, CancellationToken cancellationToken = default);
    Task UpdateAsync(ClassSession session, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
