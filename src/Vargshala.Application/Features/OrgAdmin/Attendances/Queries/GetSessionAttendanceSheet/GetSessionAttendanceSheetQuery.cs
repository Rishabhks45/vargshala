using MediatR;
using Vargshala.Application.Features.OrgAdmin.Attendances.Infrastructure;
using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Features.OrgAdmin.Attendances.Queries.GetSessionAttendanceSheet;

public record GetSessionAttendanceSheetQuery(Guid SessionId) : IRequest<ApiResponse<SessionAttendanceSheetDto>>;

public class GetSessionAttendanceSheetQueryHandler : IRequestHandler<GetSessionAttendanceSheetQuery, ApiResponse<SessionAttendanceSheetDto>>
{
    private readonly IAttendanceRepository _attendanceRepo;

    public GetSessionAttendanceSheetQueryHandler(IAttendanceRepository attendanceRepo)
    {
        _attendanceRepo = attendanceRepo;
    }

    public async Task<ApiResponse<SessionAttendanceSheetDto>> Handle(GetSessionAttendanceSheetQuery request, CancellationToken cancellationToken)
    {
        var session = await _attendanceRepo.GetSessionWithDetailsAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return ApiResponse<SessionAttendanceSheetDto>.FailureResponse("Class session not found.");
        }

        var batchStudents = await _attendanceRepo.GetEnrolledStudentsForBatchAsync(session.BatchId, cancellationToken);
        var existingAttendances = await _attendanceRepo.GetAttendancesForSessionAsync(request.SessionId, cancellationToken);
        var attendanceMap = existingAttendances.ToDictionary(a => a.StudentId);

        var sheet = new SessionAttendanceSheetDto
        {
            SessionId = session.Id,
            SessionDate = session.SessionDate,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            BatchId = session.BatchId,
            BatchName = session.Batch.Name,
            ClassName = session.Batch.Class?.Name ?? "Class",
            SubjectName = session.Batch.Subject?.Name ?? "Subject",
            TeacherName = session.Teacher?.User != null ? $"{session.Teacher.User.FirstName} {session.Teacher.User.LastName}".Trim() : null,
            Topic = session.Topic,
            SessionStatus = session.Status,
            Students = new List<StudentAttendanceItemDto>()
        };

        foreach (var bs in batchStudents)
        {
            var student = bs.Student;
            if (student == null) continue;

            var studentName = student.User != null ? $"{student.User.FirstName} {student.User.LastName}".Trim() : "Unknown Student";

            if (attendanceMap.TryGetValue(student.Id, out var existing))
            {
                sheet.Students.Add(new StudentAttendanceItemDto
                {
                    StudentId = student.Id,
                    StudentName = studentName,
                    StudentCode = student.StudentCode,
                    RollNumber = student.RollNumber,
                    AttendanceId = existing.Id,
                    Status = existing.Status,
                    Remarks = existing.Remarks,
                    MarkedAt = existing.MarkedAt
                });
            }
            else
            {
                sheet.Students.Add(new StudentAttendanceItemDto
                {
                    StudentId = student.Id,
                    StudentName = studentName,
                    StudentCode = student.StudentCode,
                    RollNumber = student.RollNumber,
                    AttendanceId = null,
                    Status = AttendanceStatus.Unmarked,
                    Remarks = null,
                    MarkedAt = null
                });
            }
        }

        return ApiResponse<SessionAttendanceSheetDto>.SuccessResponse(sheet);
    }
}
