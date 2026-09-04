using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Web.Services;

public interface IInstituteService
{
    Task<ApiResponse<List<InstituteSummaryDto>>> GetAllInstitutesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ToggleInstituteStatusAsync(Guid instituteId, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrganizationDto>> UpdateInstituteAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
}
