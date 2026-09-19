using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetBatchAttendanceOverview;

public record GetBatchAttendanceOverviewQuery(Guid BatchId, DateOnly? ReferenceDate = null) : IRequest<ApiResponse<BatchAttendanceOverviewDto>>;

public class GetBatchAttendanceOverviewQueryHandler : IRequestHandler<GetBatchAttendanceOverviewQuery, ApiResponse<BatchAttendanceOverviewDto>>
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly ICurrentUser _currentUser;

    public GetBatchAttendanceOverviewQueryHandler(
        IAttendanceRepository attendanceRepo,
        ICurrentUser currentUser)
    {
        _attendanceRepo = attendanceRepo;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BatchAttendanceOverviewDto>> Handle(GetBatchAttendanceOverviewQuery request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<BatchAttendanceOverviewDto>.FailureResponse("No active organization context found.");
        }

        var targetDate = request.ReferenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var overview = await _attendanceRepo.GetBatchOverviewAsync(request.BatchId, orgId.Value, targetDate, cancellationToken);
        if (overview == null)
        {
            return ApiResponse<BatchAttendanceOverviewDto>.FailureResponse("Batch not found.");
        }

        return ApiResponse<BatchAttendanceOverviewDto>.SuccessResponse(overview);
    }
}
