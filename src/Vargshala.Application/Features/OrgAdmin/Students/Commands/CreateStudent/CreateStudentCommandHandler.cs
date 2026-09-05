using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Students.Helpers;
using Vargshala.Application.Features.OrgAdmin.Students.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Students.Commands.CreateStudent;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, ApiResponse<StudentDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentCodeGenerator _studentCodeGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public CreateStudentCommandHandler(
        IVargshalaDbContext db,
        IStudentRepository studentRepository,
        IStudentCodeGenerator studentCodeGenerator,
        ICurrentUser currentUser,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _db = db;
        _studentRepository = studentRepository;
        _studentCodeGenerator = studentCodeGenerator;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public async Task<ApiResponse<StudentDto>> Handle(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<StudentDto>.FailureResponse("No active organization context found.");
        }

        // Auto-generate student code and roll number if not provided, or verify uniqueness against DB
        string studentCode;
        string? rollNumber = string.IsNullOrWhiteSpace(req.RollNumber) ? null : req.RollNumber.Trim();

        if (string.IsNullOrWhiteSpace(req.StudentCode))
        {
            var codeAndRoll = await _studentCodeGenerator.GenerateNextCodeAndRollAsync(cancellationToken);
            studentCode = codeAndRoll.StudentCode;
            if (string.IsNullOrWhiteSpace(rollNumber))
            {
                rollNumber = codeAndRoll.RollNumber;
            }
        }
        else
        {
            studentCode = req.StudentCode.Trim();
            var codeExists = await _studentRepository.ExistsByStudentCodeAsync(studentCode, null, cancellationToken);
            if (codeExists)
            {
                return ApiResponse<StudentDto>.FailureResponse($"A student with code '{studentCode}' already exists.");
            }

            if (string.IsNullOrWhiteSpace(rollNumber))
            {
                if (studentCode.StartsWith("STU-", StringComparison.OrdinalIgnoreCase))
                {
                    rollNumber = studentCode.Substring(4);
                }
                else
                {
                    var codeAndRoll = await _studentCodeGenerator.GenerateNextCodeAndRollAsync(cancellationToken);
                    rollNumber = codeAndRoll.RollNumber;
                }
            }
        }

        // Check duplicate email in organization if provided
        string? normalizedEmail = null;
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            normalizedEmail = req.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail && u.OrganizationId == orgId.Value && !u.IsDeleted, cancellationToken);
            if (emailExists)
            {
                return ApiResponse<StudentDto>.FailureResponse("A user with this email address already exists in this organization.");
            }
        }

        // 1. Create User
        var rawPassword = !string.IsNullOrWhiteSpace(req.Password) ? req.Password : "Student@123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId.Value,
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Email = normalizedEmail,
            Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim(),
            PasswordHash = _encryptionService.Encrypt(rawPassword, _encryptionSettings.MasterKey),
            Role = UserRole.Student,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _db.Users.AddAsync(user, cancellationToken);

        // 2. Create Student
        var student = new Student
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            DateOfBirth = req.DateOfBirth,
            Gender = req.Gender,
            BloodGroup = req.BloodGroup,
            Nationality = req.Nationality,
            StudentCode = studentCode,
            AdmissionDate = req.AdmissionDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ClassName = req.ClassName?.Trim(),
            Section = req.Section?.Trim(),
            RollNumber = rollNumber,
            FatherName = req.FatherName?.Trim(),
            FatherMobile = req.FatherMobile?.Trim(),
            FatherAlternateMobile = req.FatherAlternateMobile?.Trim(),
            MotherName = req.MotherName?.Trim(),
            Address = req.Address?.Trim(),
            City = req.City?.Trim(),
            State = req.State?.Trim(),
            PostalCode = req.PostalCode?.Trim(),
            Country = req.Country?.Trim(),
            EmergencyContactName = req.EmergencyContactName?.Trim(),
            EmergencyContactMobile = req.EmergencyContactMobile?.Trim(),
            EmergencyContactRelation = req.EmergencyContactRelation?.Trim(),
            AadharNumber = req.AadharNumber?.Trim(),
            PreviousInstitute = req.PreviousInstitute?.Trim(),
            MedicalNotes = req.MedicalNotes?.Trim(),
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _studentRepository.AddAsync(student, cancellationToken);
        await _studentRepository.SaveChangesAsync(cancellationToken);

        var dto = student.ToDto();
        return ApiResponse<StudentDto>.SuccessResponse(dto, "Student enrolled successfully.");
    }
}
