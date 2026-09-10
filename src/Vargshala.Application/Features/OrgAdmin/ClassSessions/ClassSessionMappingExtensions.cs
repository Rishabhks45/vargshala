using Vargshala.Contracts.ClassSessions;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.ClassSessions;

public static class ClassSessionMappingExtensions
{
    public static ClassSessionDto ToDto(this ClassSession s)
    {
        return new ClassSessionDto
        {
            Id = s.Id,
            BatchId = s.BatchId,
            BatchName = s.Batch?.Name ?? string.Empty,
            BatchCode = s.Batch?.Code ?? string.Empty,

            ClassId = s.Batch?.ClassId ?? Guid.Empty,
            ClassName = s.Batch?.Class?.Name ?? string.Empty,

            SubjectId = s.Batch?.SubjectId ?? Guid.Empty,
            SubjectName = s.Batch?.Subject?.Name ?? string.Empty,

            BranchId = s.Batch?.Class?.BranchId ?? Guid.Empty,
            BranchName = s.Batch?.Class?.Branch?.Name ?? string.Empty,

            BatchScheduleId = s.BatchScheduleId,
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.User != null ? $"{s.Teacher.User.FirstName} {s.Teacher.User.LastName}".Trim() : null,
            TeacherEmail = s.Teacher?.User?.Email,

            SessionDate = s.SessionDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime,

            Topic = s.Topic,
            Notes = s.Notes,
            Status = s.Status,
            IsActive = s.IsActive,

            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }
}
