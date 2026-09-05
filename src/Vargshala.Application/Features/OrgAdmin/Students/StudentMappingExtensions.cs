using Vargshala.Contracts.Students;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.OrgAdmin.Students;

public static class StudentMappingExtensions
{
    public static StudentDto ToDto(this Student student)
    {
        var u = student.User;
        return new StudentDto
        {
            Id = student.Id,
            UserId = student.UserId,
            OrganizationId = u?.OrganizationId,
            FirstName = u?.FirstName ?? string.Empty,
            LastName = u?.LastName ?? string.Empty,
            Email = u?.Email,
            Mobile = u?.Mobile,
            ProfilePictureUrl = u?.ProfilePictureUrl,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            BloodGroup = student.BloodGroup,
            Nationality = student.Nationality,
            StudentCode = student.StudentCode,
            AdmissionDate = student.AdmissionDate,
            ClassName = student.ClassName,
            Section = student.Section,
            RollNumber = student.RollNumber,
            FatherName = student.FatherName,
            FatherMobile = student.FatherMobile,
            FatherAlternateMobile = student.FatherAlternateMobile,
            MotherName = student.MotherName,
            Address = student.Address,
            City = student.City,
            State = student.State,
            PostalCode = student.PostalCode,
            Country = student.Country,
            EmergencyContactName = student.EmergencyContactName,
            EmergencyContactMobile = student.EmergencyContactMobile,
            EmergencyContactRelation = student.EmergencyContactRelation,
            AadharNumber = student.AadharNumber,
            PreviousInstitute = student.PreviousInstitute,
            MedicalNotes = student.MedicalNotes,
            IsActive = student.IsActive,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };
    }
}
