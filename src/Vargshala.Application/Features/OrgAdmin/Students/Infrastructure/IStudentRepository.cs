using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Student?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Student?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Student?> GetByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByStudentCodeAsync(string studentCode, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(List<Student> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        string? className = null,
        string? section = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(Student student, CancellationToken cancellationToken = default);
    void Update(Student student);
    void Delete(Student student);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
