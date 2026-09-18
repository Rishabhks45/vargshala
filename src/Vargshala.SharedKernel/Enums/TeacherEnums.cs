using System.ComponentModel.DataAnnotations;

namespace Vargshala.SharedKernel.Enums;

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

public enum HighestQualification
{
    [Display(Name = "B.Ed.")]
    BEd = 1,

    [Display(Name = "M.Ed.")]
    MEd = 2,

    [Display(Name = "B.Sc.")]
    BSc = 3,

    [Display(Name = "M.Sc.")]
    MSc = 4,

    [Display(Name = "B.A.")]
    BA = 5,

    [Display(Name = "M.A.")]
    MA = 6,

    [Display(Name = "B.Tech.")]
    BTech = 7,

    [Display(Name = "M.Tech.")]
    MTech = 8,

    [Display(Name = "B.Com.")]
    BCom = 9,

    [Display(Name = "M.Com.")]
    MCom = 10,

    [Display(Name = "BCA")]
    BCA = 11,

    [Display(Name = "MCA")]
    MCA = 12,

    [Display(Name = "MBA")]
    MBA = 13,

    [Display(Name = "Ph.D.")]
    PhD = 14,

    [Display(Name = "Other")]
    Other = 15
}

public static class HighestQualificationExtensions
{
    public static string GetDisplayName(this HighestQualification qualification)
        => EnumHelper.GetDisplayName(qualification);

    public static HighestQualification? ParseQualification(string? val)
        => EnumHelper.TryParseFromDisplayName<HighestQualification>(val, out var result) ? result : null;
}
