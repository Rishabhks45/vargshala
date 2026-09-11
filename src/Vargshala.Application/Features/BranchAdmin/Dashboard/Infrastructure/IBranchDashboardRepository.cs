using Vargshala.Contracts.BranchAdmin;

namespace Vargshala.Application.Features.BranchAdmin.Dashboard.Infrastructure;

public interface IBranchDashboardRepository
{
    Task<BranchDashboardDto?> GetDashboardStatsAsync(Guid organizationId, Guid branchId, CancellationToken cancellationToken = default);
}
