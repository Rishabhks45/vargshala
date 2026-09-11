using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.FeeStructures.Infrastructure;

public interface IFeeStructureRepository
{
    Task<FeeStructure?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeeStructure?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<FeeStructure> Items, int TotalRecords)> GetPagedByOrgAsync(
        Guid organizationId,
        PagedRequest request,
        Guid? branchId = null,
        Guid? classId = null,
        string? session = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task<List<FeeStructure>> GetAllActiveAsync(
        Guid organizationId,
        Guid? branchId = null,
        Guid? classId = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndBranchAsync(
        string name,
        Guid branchId,
        string session,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(FeeStructure feeStructure, CancellationToken cancellationToken = default);
    void Update(FeeStructure feeStructure);
    void Delete(FeeStructure feeStructure);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
