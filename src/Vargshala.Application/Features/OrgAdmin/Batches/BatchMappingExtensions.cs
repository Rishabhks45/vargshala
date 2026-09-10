using Vargshala.Contracts.Batches;
using Vargshala.Contracts.Teachers;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Batches;

public static class BatchMappingExtensions
{
    public static BatchDto ToDto(this Batch b)
    {
        var primaryTeacherName = b.BatchTeachers?
            .Where(bt => bt.IsActive && bt.Teacher?.User != null)
            .Select(bt => $"{bt.Teacher!.User!.FirstName} {bt.Teacher.User.LastName}".Trim())
            .FirstOrDefault() ?? string.Empty;

        var studentCount = b.BatchStudents?.Count(bs => bs.IsActive) ?? 0;
        var className = b.Class?.Name ?? string.Empty;
        var timing = b.StartTime.HasValue && b.EndTime.HasValue ? $"{b.StartTime.Value.ToString("HH:mm")} - {b.EndTime.Value.ToString("HH:mm")}" : string.Empty;

        return new BatchDto
        {
            Id = b.Id,
            ClassId = b.ClassId,
            ClassName = className,
            BranchId = b.Class?.BranchId ?? Guid.Empty,
            BranchName = b.Class?.Branch?.Name ?? string.Empty,
            SubjectId = b.SubjectId,
            SubjectName = b.Subject?.Name ?? string.Empty,
            Name = b.Name,
            Code = b.Code,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            IsActive = b.IsActive,
            AssignedTeachersCount = b.BatchTeachers?.Where(bt => bt.IsActive).Select(bt => bt.TeacherId).Distinct().Count() ?? 0,
            EnrolledStudentsCount = studentCount,
            Grade = className,
            RoomOrTiming = timing,
            PrimaryTeacher = primaryTeacherName,
            StudentCount = studentCount,
            Status = b.IsActive ? "Active" : "Inactive",
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };
    }

    public static BatchTeacherDto ToDto(this BatchTeacher bt)
    {
        var user = bt.Teacher?.User;
        return new BatchTeacherDto
        {
            Id = bt.Id,
            BatchId = bt.BatchId,
            TeacherId = bt.TeacherId,
            TeacherName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : string.Empty,
            TeacherCode = bt.Teacher?.EmployeeCode ?? string.Empty,
            TeacherEmail = user?.Email,
            TeacherMobile = user?.Mobile,
            Department = bt.Teacher?.Department,
            Designation = bt.Teacher?.Designation?.GetDisplayName(),
            Specialization = bt.Teacher?.Specialization,
            SubjectId = bt.SubjectId,
            SubjectName = bt.Subject?.Name ?? string.Empty,
            AssignedAt = bt.AssignedAt,
            RemovedAt = bt.RemovedAt,
            IsActive = bt.IsActive
        };
    }

    public static BatchStudentDto ToDto(this BatchStudent bs)
    {
        var user = bs.Student?.User;
        return new BatchStudentDto
        {
            Id = bs.Id,
            BatchId = bs.BatchId,
            StudentId = bs.StudentId,
            StudentName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : string.Empty,
            StudentCode = bs.Student?.RollNumber ?? string.Empty,
            StudentEmail = user?.Email,
            StudentMobile = user?.Mobile,
            IsPrimary = bs.IsPrimary,
            EnrollmentType = bs.EnrollmentType,
            JoinedAt = bs.JoinedAt,
            LeftAt = bs.LeftAt,
            IsActive = bs.IsActive
        };
    }
}
