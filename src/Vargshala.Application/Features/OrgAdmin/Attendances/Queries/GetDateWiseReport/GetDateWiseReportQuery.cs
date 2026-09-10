using MediatR;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetDateWiseReport;

public record GetDateWiseReportQuery(Guid BatchId, DateOnly? FromDate = null, DateOnly? ToDate = null) : IRequest<ApiResponse<List<DateAttendanceReportDto>>>;

public class GetDateWiseReportQueryHandler : IRequestHandler<GetDateWiseReportQuery, ApiResponse<List<DateAttendanceReportDto>>>
{
    private readonly IAttendanceRepository _attendanceRepo;

    public GetDateWiseReportQueryHandler(IAttendanceRepository attendanceRepo)
    {
        _attendanceRepo = attendanceRepo;
    }

    public async Task<ApiResponse<List<DateAttendanceReportDto>>> Handle(GetDateWiseReportQuery request, CancellationToken cancellationToken)
    {
        var reports = await _attendanceRepo.GetDateWiseReportAsync(request.BatchId, request.FromDate, request.ToDate, cancellationToken);
        return ApiResponse<List<DateAttendanceReportDto>>.SuccessResponse(reports);
    }
}
