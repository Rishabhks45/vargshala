using System.ComponentModel.DataAnnotations;

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
    public static string GetDisplayName(this Designation designation) => designation switch
    {
        Designation.Teacher => "Teacher",
        Designation.SeniorTeacher => "Senior Teacher",
        Designation.HOD => "HOD",
        Designation.AssistantTeacher => "Assistant Teacher",
        Designation.GuestLecturer => "Guest Lecturer",
        Designation.Principal => "Principal",
        Designation.VicePrincipal => "Vice Principal",
        Designation.LabAssistant => "Lab Assistant",
        Designation.Other => "Other",
        _ => designation.ToString()
    };

    public static Designation? ParseDesignation(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;

        if (int.TryParse(val, out var intVal) && Enum.IsDefined(typeof(Designation), intVal))
            return (Designation)intVal;

        var clean = val.Trim().Replace(" ", "").Replace("-", "");
        foreach (Designation d in Enum.GetValues(typeof(Designation)))
        {
            if (string.Equals(d.ToString(), clean, StringComparison.OrdinalIgnoreCase))
                return d;
            if (string.Equals(d.GetDisplayName().Replace(" ", ""), clean, StringComparison.OrdinalIgnoreCase))
                return d;
        }

        return null;
    }
}
