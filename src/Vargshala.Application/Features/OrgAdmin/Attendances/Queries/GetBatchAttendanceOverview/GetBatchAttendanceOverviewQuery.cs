using MediatR;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetBatchAttendanceOverview;

public record GetBatchAttendanceOverviewQuery(Guid BatchId, DateOnly? ReferenceDate = null) : IRequest<ApiResponse<BatchAttendanceOverviewDto>>;

public class GetBatchAttendanceOverviewQueryHandler : IRequestHandler<GetBatchAttendanceOverviewQuery, ApiResponse<BatchAttendanceOverviewDto>>
{
    private readonly IAttendanceRepository _attendanceRepo;

    public GetBatchAttendanceOverviewQueryHandler(IAttendanceRepository attendanceRepo)
    {
        _attendanceRepo = attendanceRepo;
    }

    public async Task<ApiResponse<BatchAttendanceOverviewDto>> Handle(GetBatchAttendanceOverviewQuery request, CancellationToken cancellationToken)
    {
        var targetDate = request.ReferenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var overview = await _attendanceRepo.GetBatchOverviewAsync(request.BatchId, targetDate, cancellationToken);
        if (overview == null)
        {
            return ApiResponse<BatchAttendanceOverviewDto>.FailureResponse("Batch not found.");
        }

        return ApiResponse<BatchAttendanceOverviewDto>.SuccessResponse(overview);
    }
}
