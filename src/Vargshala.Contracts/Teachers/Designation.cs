using System.ComponentModel.DataAnnotations;
using Vargshala.Contracts.Common;

namespace Vargshala.Contracts.Teachers;

public enum Designation
{
    [Display(Name = "Teacher")]
    Teacher = 1,

    [Display(Name = "Senior Teacher")]
    SeniorTeacher = 2,

    [Display(Name = "HOD")]
    HOD = 3,

    [Display(Name = "Assistant Teacher")]
    AssistantTeacher = 4,

    [Display(Name = "Guest Lecturer")]
    GuestLecturer = 5,

    [Display(Name = "Principal")]
    Principal = 6,

    [Display(Name = "Vice Principal")]
    VicePrincipal = 7,

    [Display(Name = "Lab Assistant")]
    LabAssistant = 8,

    [Display(Name = "Other")]
    Other = 9
}

public static class DesignationExtensions
{
    public static string GetDisplayName(this Designation designation)
        => EnumHelper.GetDisplayName(designation);

    public static Designation? ParseDesignation(string? val)
        => EnumHelper.TryParseFromDisplayName<Designation>(val, out var result) ? result : null;
}
