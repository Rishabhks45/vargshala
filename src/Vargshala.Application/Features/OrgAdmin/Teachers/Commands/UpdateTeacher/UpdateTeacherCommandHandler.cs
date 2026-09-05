using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Commands.UpdateTeacher;

public class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, ApiResponse<TeacherDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateTeacherCommandHandler(
        IVargshalaDbContext db,
        ITeacherRepository teacherRepository,
        ICurrentUser currentUser)
    {
        _db = db;
        _teacherRepository = teacherRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<TeacherDto>> Handle(
        UpdateTeacherCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var teacher = await _teacherRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (teacher == null)
        {
            return ApiResponse<TeacherDto>.FailureResponse("Teacher not found.");
        }

        var orgId = _currentUser.OrganizationId;
        if (orgId.HasValue && teacher.User?.OrganizationId != null && teacher.User.OrganizationId != orgId.Value)
        {
            return ApiResponse<TeacherDto>.FailureResponse("Unauthorized to update this teacher.");
        }

        // Check duplicate employee code
        if (!string.IsNullOrWhiteSpace(req.EmployeeCode))
        {
            var codeExists = await _teacherRepository.ExistsByEmployeeCodeAsync(req.EmployeeCode.Trim(), teacher.Id, cancellationToken);
            if (codeExists)
            {
                return ApiResponse<TeacherDto>.FailureResponse($"A teacher with code '{req.EmployeeCode}' already exists.");
            }
        }

        // Check duplicate email
        if (!string.IsNullOrWhiteSpace(req.Email) && teacher.User != null)
        {
            var normalizedEmail = req.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Users
                .AnyAsync(u => u.Id != teacher.UserId && u.Email != null && u.Email.ToLower() == normalizedEmail && u.OrganizationId == teacher.User.OrganizationId && !u.IsDeleted, cancellationToken);
            if (emailExists)
            {
                return ApiResponse<TeacherDto>.FailureResponse("A user with this email address already exists in this organization.");
            }
        }

        // Update User
        if (teacher.User != null)
        {
            teacher.User.FirstName = req.FirstName.Trim();
            teacher.User.LastName = req.LastName.Trim();
            teacher.User.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim().ToLowerInvariant();
            teacher.User.Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim();
            teacher.User.IsActive = req.IsActive;
            teacher.User.UpdatedAt = DateTime.UtcNow;
            teacher.User.UpdatedBy = _currentUser.UserId;
        }

        // Update Teacher
        teacher.EmployeeCode = req.EmployeeCode?.Trim();
        teacher.JoiningDate = req.JoiningDate;
        teacher.Department = req.Department?.Trim();
        teacher.Designation = req.Designation?.Trim();
        teacher.HighestQualification = req.HighestQualification?.Trim();
        teacher.Specialization = req.Specialization?.Trim();
        teacher.TeachingExperienceYears = req.TeachingExperienceYears;
        teacher.Address = req.Address?.Trim();
        teacher.City = req.City?.Trim();
        teacher.State = req.State?.Trim();
        teacher.PostalCode = req.PostalCode?.Trim();
        teacher.Country = req.Country?.Trim();
        teacher.AadharNumber = req.AadharNumber?.Trim();
        teacher.PreviousInstitute = req.PreviousInstitute?.Trim();
        teacher.Bio = req.Bio?.Trim();
        teacher.IsActive = req.IsActive;
        teacher.UpdatedAt = DateTime.UtcNow;
        teacher.UpdatedBy = _currentUser.UserId;

        _teacherRepository.Update(teacher);
        await _teacherRepository.SaveChangesAsync(cancellationToken);

        var dto = teacher.ToDto();
        return ApiResponse<TeacherDto>.SuccessResponse(dto, "Teacher profile updated successfully.");
    }
}
