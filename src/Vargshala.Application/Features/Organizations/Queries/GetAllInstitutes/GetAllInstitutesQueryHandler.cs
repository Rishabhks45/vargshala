using MediatR;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Organizations;

namespace Vargshala.Application.Features.Organizations.Queries.GetAllInstitutes;

public class GetAllInstitutesQueryHandler : IRequestHandler<GetAllInstitutesQuery, ApiResponse<PagedResponse<InstituteSummaryDto>>>
{
    #region Fields & Constructor
    private readonly IOrganizationRepository _organizationRepository;

    public GetAllInstitutesQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }
    #endregion

    #region Query Handler
    public async Task<ApiResponse<PagedResponse<InstituteSummaryDto>>> Handle(
        GetAllInstitutesQuery request,
        CancellationToken cancellationToken)
    {
        var pagedRequest = request.Request ?? new PagedRequest();

        var (items, totalRecords) = await _organizationRepository.GetInstitutesSummaryPagedAsync(pagedRequest, cancellationToken);

        var response = PagedResponse<InstituteSummaryDto>.Create(items, totalRecords, pagedRequest.PageNumber, pagedRequest.PageSize);
        return ApiResponse<PagedResponse<InstituteSummaryDto>>.SuccessResponse(response);
    }
    #endregion
}
