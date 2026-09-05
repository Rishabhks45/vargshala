using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;

public interface ITeacherRepository
{
    Task<Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Teacher?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Teacher?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmployeeCodeAsync(string employeeCode, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(List<Teacher> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        string? department = null,
        string? designation = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(Teacher teacher, CancellationToken cancellationToken = default);
    void Update(Teacher teacher);
    void Delete(Teacher teacher);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
