using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetStudentWiseReport;

public record GetStudentWiseReportQuery(Guid BatchId) : IRequest<ApiResponse<List<StudentAttendanceReportDto>>>;

public class GetStudentWiseReportQueryHandler : IRequestHandler<GetStudentWiseReportQuery, ApiResponse<List<StudentAttendanceReportDto>>>
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly ICurrentUser _currentUser;

    public GetStudentWiseReportQueryHandler(
        IAttendanceRepository attendanceRepo,
        ICurrentUser currentUser)
    {
        _attendanceRepo = attendanceRepo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<List<StudentAttendanceReportDto>>> Handle(GetStudentWiseReportQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<List<StudentAttendanceReportDto>>.FailureResponse("No active organization context found.");
        }

        var reports = await _attendanceRepo.GetStudentWiseReportAsync(request.BatchId, orgId.Value, cancellationToken);
        return ApiResponse<List<StudentAttendanceReportDto>>.SuccessResponse(reports);
    }
}
