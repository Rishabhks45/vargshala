using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Subjects.Infrastructure;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Subject?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAndOrgAsync(string code, Guid organizationId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(List<Subject> Items, int TotalRecords)> GetPagedByOrgAsync(Guid organizationId, PagedRequest request, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<List<Subject>> GetAllActiveByOrgAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(Subject subject, CancellationToken cancellationToken = default);
    void Update(Subject subject);
    void Delete(Subject subject);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
