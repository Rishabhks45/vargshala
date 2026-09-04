using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Organizations.Infrastructure;

#region Interface
public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Organization?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);
    void Update(Organization organization);
    void Delete(Organization organization);
    Task<List<Organization>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(List<Organization> Items, int TotalRecords)> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<(List<InstituteSummaryDto> Items, int TotalRecords)> GetInstitutesSummaryPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<Organization?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
#endregion
