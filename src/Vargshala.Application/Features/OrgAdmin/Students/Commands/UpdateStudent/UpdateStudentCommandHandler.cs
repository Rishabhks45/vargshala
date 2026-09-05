using MediatR;
using Microsoft.EntityFrameworkCore;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Commands.UpdateStudent;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, ApiResponse<StudentDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateStudentCommandHandler(
        IVargshalaDbContext db,
        IStudentRepository studentRepository,
        ICurrentUser currentUser)
    {
        _db = db;
        _studentRepository = studentRepository;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<StudentDto>> Handle(
        UpdateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var student = await _studentRepository.GetByIdForUpdateAsync(req.Id, cancellationToken);
        if (student == null)
        {
            return ApiResponse<StudentDto>.FailureResponse("Student not found.");
        }

        var orgId = _currentUser.OrganizationId;
        if (orgId.HasValue && student.User?.OrganizationId != null && student.User.OrganizationId != orgId.Value)
        {
            return ApiResponse<StudentDto>.FailureResponse("Unauthorized to update this student.");
        }

        // Check duplicate student code within this organization
        if (!string.IsNullOrWhiteSpace(req.StudentCode))
        {
            var codeExists = await _studentRepository.ExistsByStudentCodeAsync(req.StudentCode.Trim(), student.Id, cancellationToken);
            if (codeExists)
            {
                return ApiResponse<StudentDto>.FailureResponse($"A student with code '{req.StudentCode}' already exists.");
            }
        }

        // Check duplicate email
        if (!string.IsNullOrWhiteSpace(req.Email) && student.User != null)
        {
            var normalizedEmail = req.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Users
                .AnyAsync(u => u.Id != student.UserId && u.Email != null && u.Email.ToLower() == normalizedEmail && u.OrganizationId == student.User.OrganizationId && !u.IsDeleted, cancellationToken);
            if (emailExists)
            {
                return ApiResponse<StudentDto>.FailureResponse("A user with this email address already exists in this organization.");
            }
        }

        // Update User
        if (student.User != null)
        {
            student.User.FirstName = req.FirstName.Trim();
            student.User.LastName = req.LastName.Trim();
            student.User.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim().ToLowerInvariant();
            student.User.Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim();
            student.User.IsActive = req.IsActive;
            student.User.UpdatedAt = DateTime.UtcNow;
            student.User.UpdatedBy = _currentUser.UserId;
        }

        // Update Student
        student.DateOfBirth = req.DateOfBirth;
        student.Gender = req.Gender;
        student.BloodGroup = req.BloodGroup;
        student.Nationality = req.Nationality;
        student.StudentCode = req.StudentCode?.Trim();
        student.AdmissionDate = req.AdmissionDate;
        student.ClassName = req.ClassName?.Trim();
        student.Section = req.Section?.Trim();
        student.RollNumber = req.RollNumber?.Trim();
        student.FatherName = req.FatherName?.Trim();
        student.FatherMobile = req.FatherMobile?.Trim();
        student.FatherAlternateMobile = req.FatherAlternateMobile?.Trim();
        student.MotherName = req.MotherName?.Trim();
        student.Address = req.Address?.Trim();
        student.City = req.City?.Trim();
        student.State = req.State?.Trim();
        student.PostalCode = req.PostalCode?.Trim();
        student.Country = req.Country?.Trim();
        student.EmergencyContactName = req.EmergencyContactName?.Trim();
        student.EmergencyContactMobile = req.EmergencyContactMobile?.Trim();
        student.EmergencyContactRelation = req.EmergencyContactRelation?.Trim();
        student.AadharNumber = req.AadharNumber?.Trim();
        student.PreviousInstitute = req.PreviousInstitute?.Trim();
        student.MedicalNotes = req.MedicalNotes?.Trim();
        student.IsActive = req.IsActive;
        student.UpdatedAt = DateTime.UtcNow;
        student.UpdatedBy = _currentUser.UserId;

        _studentRepository.Update(student);
        await _studentRepository.SaveChangesAsync(cancellationToken);

        var dto = student.ToDto();
        return ApiResponse<StudentDto>.SuccessResponse(dto, "Student profile updated successfully.");
    }
}
