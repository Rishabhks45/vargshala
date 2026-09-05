using Vargshala.Contracts.Teachers;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Teachers;

public static class TeacherMappingExtensions
{
    public static TeacherDto ToDto(this Teacher teacher)
    {
        var u = teacher.User;
        return new TeacherDto
        {
            Id = teacher.Id,
            UserId = teacher.UserId,
            OrganizationId = u?.OrganizationId,
            FirstName = u?.FirstName ?? string.Empty,
            LastName = u?.LastName ?? string.Empty,
            Email = u?.Email,
            Mobile = u?.Mobile,
            ProfilePictureUrl = u?.ProfilePictureUrl,
            EmployeeCode = teacher.EmployeeCode,
            JoiningDate = teacher.JoiningDate,
            Department = teacher.Department,
            Designation = teacher.Designation,
            HighestQualification = teacher.HighestQualification,
            Specialization = teacher.Specialization,
            TeachingExperienceYears = teacher.TeachingExperienceYears,
            Address = teacher.Address,
            City = teacher.City,
            State = teacher.State,
            PostalCode = teacher.PostalCode,
            Country = teacher.Country,
            AadharNumber = teacher.AadharNumber,
            PreviousInstitute = teacher.PreviousInstitute,
            Bio = teacher.Bio,
            IsActive = teacher.IsActive,
            CreatedAt = teacher.CreatedAt,
            UpdatedAt = teacher.UpdatedAt
        };
    }
}
