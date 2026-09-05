using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Branches.Infrastructure;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Branch?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(List<Branch> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        string? city = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task<List<Branch>> GetAllActiveByOrgAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<List<UserBranchAccess>> GetUserBranchesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AssignUserBranchesAsync(Guid userId, List<Guid> branchIds, Guid? createdBy = null, CancellationToken cancellationToken = default);
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
    void Update(Branch branch);
    void Delete(Branch branch);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
