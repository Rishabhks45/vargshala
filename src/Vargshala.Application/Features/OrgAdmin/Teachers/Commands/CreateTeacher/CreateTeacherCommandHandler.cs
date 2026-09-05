using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vargshala.Application.Abstractions.Authentication;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Persistence;
using Vargshala.Application.Features.OrgAdmin.Teachers.Helpers;
using Vargshala.Application.Features.OrgAdmin.Teachers.Infrastructure;
using Vargshala.Application.Settings;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Commands.CreateTeacher;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, ApiResponse<TeacherDto>>
{
    private readonly IVargshalaDbContext _db;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IEmployeeCodeGenerator _employeeCodeGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly EncryptionSettings _encryptionSettings;

    public CreateTeacherCommandHandler(
        IVargshalaDbContext db,
        ITeacherRepository teacherRepository,
        IEmployeeCodeGenerator employeeCodeGenerator,
        ICurrentUser currentUser,
        IEncryptionService encryptionService,
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _db = db;
        _teacherRepository = teacherRepository;
        _employeeCodeGenerator = employeeCodeGenerator;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
        _encryptionSettings = encryptionOptions.Value;
    }

    public async Task<ApiResponse<TeacherDto>> Handle(
        CreateTeacherCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<TeacherDto>.FailureResponse("No active organization context found.");
        }

        // Auto-generate employee code if not provided, or verify uniqueness against DB
        string employeeCode;
        if (string.IsNullOrWhiteSpace(req.EmployeeCode))
        {
            employeeCode = await _employeeCodeGenerator.GenerateNextCodeAsync(cancellationToken);
        }
        else
        {
            employeeCode = req.EmployeeCode.Trim();
            var codeExists = await _teacherRepository.ExistsByEmployeeCodeAsync(employeeCode, null, cancellationToken);
            if (codeExists)
            {
                return ApiResponse<TeacherDto>.FailureResponse($"A teacher with code '{employeeCode}' already exists.");
            }
        }

        // Check duplicate email
        string? normalizedEmail = null;
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            normalizedEmail = req.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail && u.OrganizationId == orgId.Value && !u.IsDeleted, cancellationToken);
            if (emailExists)
            {
                return ApiResponse<TeacherDto>.FailureResponse("A user with this email address already exists in this organization.");
            }
        }

        // 1. Create User
        var rawPassword = !string.IsNullOrWhiteSpace(req.Password) ? req.Password : "Teacher@123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId.Value,
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            Email = normalizedEmail,
            Mobile = string.IsNullOrWhiteSpace(req.Mobile) ? null : req.Mobile.Trim(),
            PasswordHash = _encryptionService.Encrypt(rawPassword, _encryptionSettings.MasterKey),
            Role = UserRole.Teacher,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _db.Users.AddAsync(user, cancellationToken);

        // 2. Create Teacher
        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            EmployeeCode = employeeCode,
            JoiningDate = req.JoiningDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Department = req.Department?.Trim(),
            Designation = req.Designation?.Trim(),
            HighestQualification = req.HighestQualification?.Trim(),
            Specialization = req.Specialization?.Trim(),
            TeachingExperienceYears = req.TeachingExperienceYears,
            Address = req.Address?.Trim(),
            City = req.City?.Trim(),
            State = req.State?.Trim(),
            PostalCode = req.PostalCode?.Trim(),
            Country = req.Country?.Trim(),
            AadharNumber = req.AadharNumber?.Trim(),
            PreviousInstitute = req.PreviousInstitute?.Trim(),
            Bio = req.Bio?.Trim(),
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUser.UserId
        };

        await _teacherRepository.AddAsync(teacher, cancellationToken);
        await _teacherRepository.SaveChangesAsync(cancellationToken);

        var dto = teacher.ToDto();
        return ApiResponse<TeacherDto>.SuccessResponse(dto, "Teacher added successfully.");
    }
}
