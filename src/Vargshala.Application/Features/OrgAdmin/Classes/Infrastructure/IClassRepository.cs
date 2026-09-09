using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Classes.Infrastructure;

public interface IClassRepository
{
    Task<Class?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Class?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAndBranchAsync(string code, Guid branchId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasBatchesAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<(List<Class> Items, int TotalRecords)> GetPagedAsync(Guid organizationId, PagedRequest request, Guid? branchId = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<List<Class>> GetAllActiveAsync(Guid organizationId, Guid? branchId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Class entity, CancellationToken cancellationToken = default);
    void Update(Class entity);
    void Delete(Class entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
