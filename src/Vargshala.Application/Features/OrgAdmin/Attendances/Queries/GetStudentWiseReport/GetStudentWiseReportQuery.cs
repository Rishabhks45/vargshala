using MediatR;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetStudentWiseReport;

public record GetStudentWiseReportQuery(Guid BatchId) : IRequest<ApiResponse<List<StudentAttendanceReportDto>>>;

public class GetStudentWiseReportQueryHandler : IRequestHandler<GetStudentWiseReportQuery, ApiResponse<List<StudentAttendanceReportDto>>>
{
    private readonly IAttendanceRepository _attendanceRepo;

    public GetStudentWiseReportQueryHandler(IAttendanceRepository attendanceRepo)
    {
        _attendanceRepo = attendanceRepo;
    }

    public async Task<ApiResponse<List<StudentAttendanceReportDto>>> Handle(GetStudentWiseReportQuery request, CancellationToken cancellationToken)
    {
        var reports = await _attendanceRepo.GetStudentWiseReportAsync(request.BatchId, cancellationToken);
        return ApiResponse<List<StudentAttendanceReportDto>>.SuccessResponse(reports);
    }
}
