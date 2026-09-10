using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetSessionAttendanceSheet;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Commands.SaveSessionAttendance;

public record SaveSessionAttendanceCommand(MarkSessionAttendanceRequest Request) : IRequest<ApiResponse<SessionAttendanceSheetDto>>;

public class SaveSessionAttendanceCommandHandler : IRequestHandler<SaveSessionAttendanceCommand, ApiResponse<SessionAttendanceSheetDto>>
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public SaveSessionAttendanceCommandHandler(
        IAttendanceRepository attendanceRepo,
        ICurrentUser currentUser,
        ISender sender)
    {
        _attendanceRepo = attendanceRepo;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<ApiResponse<SessionAttendanceSheetDto>> Handle(SaveSessionAttendanceCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var session = await _attendanceRepo.GetSessionWithDetailsAsync(req.ClassSessionId, cancellationToken);
        if (session == null)
        {
            return ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Class session not found.");
        }

        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var item in req.Items)
        {
            var validStatus = AttendanceStatus.Normalize(item.Status);
            var existing = await _attendanceRepo.GetAttendanceAsync(req.ClassSessionId, item.StudentId, cancellationToken);

            if (existing != null)
            {
                existing.Status = validStatus;
                existing.Remarks = item.Remarks;
                existing.MarkedAt = now;
                existing.UpdatedAt = now;
                existing.UpdatedBy = userId;
                _attendanceRepo.Update(existing);
            }
            else
            {
                var newAttendance = new Attendance
                {
                    Id = Guid.NewGuid(),
                    ClassSessionId = req.ClassSessionId,
                    StudentId = item.StudentId,
                    Status = validStatus,
                    Remarks = item.Remarks,
                    MarkedAt = now,
                    CreatedAt = now,
                    CreatedBy = userId,
                    IsDeleted = false
                };
                await _attendanceRepo.AddAsync(newAttendance, cancellationToken);
            }
        }

        await _attendanceRepo.SaveChangesAsync(cancellationToken);

        // Return refreshed sheet
        return await _sender.Send(new GetSessionAttendanceSheetQuery(req.ClassSessionId), cancellationToken);
    }
}
