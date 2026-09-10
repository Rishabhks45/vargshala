using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Batches.Infrastructure;

public interface IBatchRepository
{
    Task<Batch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Batch?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Batch?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAndClassAsync(string code, Guid classId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(List<Batch> Items, int TotalRecords)> GetPagedAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        Guid? subjectId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task<List<Batch>> GetAllActiveAsync(Guid organizationId, Guid? classId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Batch batch, CancellationToken cancellationToken = default);
    void Update(Batch batch);
    void Delete(Batch batch);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Teacher assignment
    Task<List<BatchTeacher>> GetTeachersByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<BatchTeacher?> GetBatchTeacherAsync(Guid batchId, Guid teacherId, CancellationToken cancellationToken = default);
    Task AddTeacherAsync(BatchTeacher batchTeacher, CancellationToken cancellationToken = default);
    void UpdateTeacher(BatchTeacher batchTeacher);

    // Student enrollment
    Task<List<BatchStudent>> GetStudentsByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<BatchStudent?> GetBatchStudentAsync(Guid batchId, Guid studentId, CancellationToken cancellationToken = default);
    Task<List<BatchStudent>> GetBatchesByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task AddStudentAsync(BatchStudent batchStudent, CancellationToken cancellationToken = default);
    void UpdateStudent(BatchStudent batchStudent);
}
