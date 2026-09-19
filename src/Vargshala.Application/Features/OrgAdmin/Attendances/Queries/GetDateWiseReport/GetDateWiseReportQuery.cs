using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetDateWiseReport;

public record GetDateWiseReportQuery(Guid BatchId, DateOnly? FromDate = null, DateOnly? ToDate = null) : IRequest<ApiResponse<List<DateAttendanceReportDto>>>;

public class GetDateWiseReportQueryHandler : IRequestHandler<GetDateWiseReportQuery, ApiResponse<List<DateAttendanceReportDto>>>
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly ICurrentUser _currentUser;

    public GetDateWiseReportQueryHandler(
        IAttendanceRepository attendanceRepo,
        ICurrentUser currentUser)
    {
        _attendanceRepo = attendanceRepo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<DateAttendanceReportDto>>> Handle(GetDateWiseReportQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<DateAttendanceReportDto>>.FailureResponse("No active organization context found.");
        }

        var reports = await _attendanceRepo.GetDateWiseReportAsync(request.BatchId, orgId.Value, request.FromDate, request.ToDate, cancellationToken);
        return ApiResponse<List<DateAttendanceReportDto>>.SuccessResponse(reports);
    }
}
